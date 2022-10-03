// Program: FN_B664_WRITE_AR_STATEMENT, ID: 371234091, model: 746.
// Short name: SWE02021
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B664_WRITE_AR_STATEMENT.
/// </summary>
[Serializable]
public partial class FnB664WriteArStatement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B664_WRITE_AR_STATEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB664WriteArStatement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB664WriteArStatement.
  /// </summary>
  public FnB664WriteArStatement(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 02/17/05  GVandy	PR233867	Initial Development.  New business rules for AR
    // statements.
    // 04/18/05  GVandy	PR241535	Correct display of amounts retained greater 
    // than 999.99
    // 05/02/05  GVandy	PR243099	AR address line 2 is not displaying on the 
    // statement.
    // 05/02/05  GVandy	PR243087	Use only active verified AR addresses.
    // 05/02/05  GVandy	PR243273	Add message 'This page left blank 
    // intentionally' if the only info on the last page
    // 					is the footer disclaimer.
    // 05/02/05  GVandy	PR242550	Do not send statement if the AR is deceased.
    // 05/02/05  GVandy	PR242288	Do not send statement if the only collection 
    // activity is for 718B judgements.
    // 05/17/05  GVandy	PR244676	'This page left blank intentionally' not 
    // displaying in one situation.
    // 12/10/10  RMathews      CQ22192         Expand amount fields from 5.2 to 
    // 6.2
    // 06/19/12  AHockman   CQ33636 & CQ8056   Agency name change & remove/
    // change phone #'s.
    // 11/21/13  JHarden    CQ95556            Change the line with phone number
    // in statement.
    // 05/06/16  JHarden    CQ51922            Hard code Office Name and 
    // Address.
    // 2/10/19    AHockman    cq65157          change Office name / address back
    // to local
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  Create the AR Statement using the data extracted by B663 which has 
    // been externally sorted/summed.
    // --
    // --  The format of the report is listed below.  Line numbers on the left 
    // and column numbers on top and bottom are for
    // --  reference only and will not appear on the statement.
    // --  Leader dots (...) are used only to illustrate the maximum size of the
    // field. They will not appear on the statement.
    // 06-19-2012 edited statement to remove local office phone    AHockman
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  START OF AR STATEMENT EXAMPLE
    // --
    // --
    // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
    // --         1         2         3         4         5         6         7
    // 8
    // -------------------------------------------------------------------------------------------------------------------------
    // 1                  Child Support Enforcement.....
    //  2     ____         PO Box 246...............
    //  3                  Topeka, KS 66601-xxxx-xxx.........    Statement 
    // Period:
    //  4
    // 
    // 02/01/2005 to 02/28/2005
    //  5
    //  6
    // 
    // Payments received after
    //  7     ____         Suzie Bear...................      this date will not
    // appear
    //  8     ____         121 Wilson Street........          on this statement.
    //  9                  Topeka, KS 66601-xxxx-xxx.........
    // 10
    // 11
    // 12
    // 13
    // 14                  KANSAS DEPARTMENT FOR CHILDREN AND FAMILIES
    // 15                            CHILD SUPPORT SERVICES
    // 16
    // 17     This statement is for your information only.  No action is needed 
    // from
    // 18     you at this time.  This statement may list payments that were not 
    // sent
    // 19     to you.  A payment may be kept to repay the State for public 
    // assistance
    // 20     that you received.  Automated payment history is available 24 
    // hours a day
    // 21     by contacting the Kansas Child Support Call Center by phone at
    // 22     1-888-7-KS-CHILD (1-888-757-2445) or TTY 1-888-688-1666.
    //        "OLD"  To view your payment history, please visit the Kansas
    //        Payment Center at www.kspaycenter.com or contact Kansas Child 
    // Support Call
    //        Center  by phone at 1-888-7-KS-CHILD (1-888-727-2445) or TTY 1-888
    // -688-1666.
    // 23
    // 24
    // 25            Payor Name/                 Person ID/  Court Order/
    // 26            Date           Amount       Support     Amount Kept   
    // Amount Sent
    // 27            Collected      Collected    Type        By State      To 
    // Family
    // 28
    // 29            John J. Smith.............. 0000030144  SN01D 000001
    // 30            02/15/2005     $   562.13   Current                   $   
    // 298.56
    // 31
    // 
    // Arrears                   $   263.57
    // 32
    // 33            Lenny K. Wilson............ 0000515222  WY03D 000245
    // 34            02/01/2005     $   152.00   Current     $   107.00
    // 35
    // 
    // Arrears     $    45.00
    // 36
    // 37
    // 38
    // 
    // ---------
    // 39
    // 
    // Total sent to the family:
    // $    562.13
    // 40
    // 41
    // 42
    // 43
    // 44
    // 45
    // 46
    // 47
    // 48
    // 49
    // 50
    // 51
    // 52
    // 53
    // 54
    // 55
    // 56
    // 57     Due to fees and other adjustments, the amount received may be less
    // 58     than what is shown.  Support Type: Current - support due in 
    // current
    // 59     month, Arrears - support past due.
    // -------------------------------------------------------------------------------------------------------------------------
    // --
    // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
    // --         1         2         3         4         5         6         7
    // 8
    // --
    // --  END OF AR STATEMENT EXAMPLE
    // -------------------------------------------------------------------------------------------------------------------------
    // --  The export group is only used to view match to the local group in the
    // PrAD so that it is re-initialized after the AR statement is printed, it
    // is otherwise not referenced in this cab.
    // -- If the import group is empty then escape out.  This is used to re-
    // initialize the group in the PrAD when an AR statement exceeded the
    // maximum entries in the group view.
    if (import.Import1.IsEmpty)
    {
      return;
    }

    // -- Default return status to ERRORED.  We'll reset it to SKIPPED or 
    // PRINTED later.
    export.ArStatementStatus.Text8 = "ERRORED";
    local.LastOfImportGroup.Count = import.Import1.Count;

    // -- Define the number of lines to be printed on each page of the AR 
    // statement.
    local.NumberOfLinesPerPage.Count = 59;

    if (!ReadCsePerson1())
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CSE Person Not Found.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // 05/02/05  GVandy  PR242550  Do not send statement if the AR is deceased.
    if (!Equal(entities.Ar.DateOfDeath, local.Null1.Date))
    {
      export.ArStatementStatus.Text8 = "DECEASED";

      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - AR is Deceased.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // 05/02/05  GVandy  PR242288  Do not send statement if the only collection 
    // activity is for 718B judgements.
    if (import.Non718BCollection.Count == 0)
    {
      // --  B663 writes obligation type of 0 to the extract file for 718B 
      // collections.  The sort/sum step then sums
      // --  the obligation types on the ARs collections and B664 will not print
      // a statement if the summed obligation
      // --  types for the AR equal 0 (meaning only 718B collections were found
      // ).
      export.ArStatementStatus.Text8 = "718BONLY";

      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - 718B Collections Only.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Find a case for which assigned obligations exist for the AR.
    // -------------------------------------------------------------------------------------------------------------------------
    local.CaseFound.Flag = "N";

    foreach(var item in ReadCase())
    {
      local.CaseFound.Flag = "Y";

      // -- Insure there is an assigned obligation to this AR on this case.
      foreach(var item1 in ReadAccrualInstructions())
      {
        if (!ReadDebt1())
        {
          continue;
        }

        if (ReadObligation())
        {
          if (AsChar(entities.Obligation.OrderTypeCode) != 'K')
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        if (!ReadCsePerson2())
        {
          continue;
        }

        if (!ReadObligationType())
        {
          continue;
        }

        if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
        {
          if (!ReadCsePerson3())
          {
            continue;
          }
        }

        if (ReadDebtDetail1())
        {
          local.DebtDetail.Assign(entities.DebtDetail);
        }

        local.DebtDetail.DueDt = import.ProgramProcessingInfo.ProcessDate;
        UseFnDeterminePgmForDebtDetail2();

        if (Equal(local.Program.Code, "AF") || Equal
          (local.Program.Code, "NA") && Equal
          (local.DprProgram.ProgramState, "CA"))
        {
          // --  Insure the AR for the debt is the same as the AR we are 
          // processing.
          local.DueDate.Date = import.ProgramProcessingInfo.ProcessDate;
          UseFnB664DetermineAr();

          if (Equal(entities.Ar.Number, local.DerivedAr.Number))
          {
            // -- An assigned obligation exists on this case for our AR.  Use 
            // this case.
            goto ReadEach;
          }
        }
      }

      foreach(var item1 in ReadDebtDetail2())
      {
        if (!ReadDebt2())
        {
          continue;
        }

        if (ReadObligation())
        {
          if (AsChar(entities.Obligation.OrderTypeCode) != 'K')
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        if (!ReadCsePerson2())
        {
          continue;
        }

        if (!ReadObligationType())
        {
          continue;
        }

        if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
        {
          if (!ReadCsePerson3())
          {
            continue;
          }
        }

        UseFnDeterminePgmForDebtDetail1();

        if (Equal(local.Program.Code, "AF") || Equal
          (local.Program.Code, "NA") && Equal
          (local.DprProgram.ProgramState, "CA"))
        {
          // --  Insure the AR for the debt is the same as the AR we are 
          // processing.
          local.DueDate.Date = entities.DebtDetail.DueDt;
          UseFnB664DetermineAr();

          if (Equal(entities.Ar.Number, local.DerivedAr.Number))
          {
            // -- An assigned obligation exists on this case for our AR.  Use 
            // this case.
            goto ReadEach;
          }
        }
      }
    }

ReadEach:

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Determine if a case was found with assigned obligations.
    // -------------------------------------------------------------------------------------------------------------------------
    if (!entities.Case1.Populated)
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";

      if (AsChar(local.CaseFound.Flag) == 'Y')
      {
        // -- A case was found for the AR but there were no assigned 
        // obligations.  Skip the AR statement.
        export.ArStatementStatus.Text8 = "NOASSOB";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No Assigned Obligations.";
          
      }
      else
      {
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No Open CSE Case.";
          
      }

      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Determine office address & phone number for the statement.
    // -------------------------------------------------------------------------------------------------------------------------
    ReadOfficeOfficeAddress();

    if (!entities.Office.Populated || !entities.OfficeAddress1.Populated)
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CSE Office Not Found.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Format the Office address.
    // -------------------------------------------------------------------------------------------------------------------------
    // cq65157  2/2019   changing return office address back to field address 
    // from hard
    //                   coded central office address.    Anita Hockman
    local.SpPrintWorkSet.LocationType = "";
    local.SpPrintWorkSet.City = entities.OfficeAddress1.City;
    local.SpPrintWorkSet.State = entities.OfficeAddress1.StateProvince;
    local.SpPrintWorkSet.Street1 = entities.OfficeAddress1.Street1;
    local.SpPrintWorkSet.Street2 = entities.OfficeAddress1.Street2 ?? Spaces
      (25);
    local.SpPrintWorkSet.ZipCode = entities.OfficeAddress1.Zip ?? Spaces(5);
    local.SpPrintWorkSet.Zip3 = entities.OfficeAddress1.Zip3 ?? Spaces(3);
    local.SpPrintWorkSet.Zip4 = entities.OfficeAddress1.Zip4 ?? Spaces(4);
    UseSpDocFormatAddress2();

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read and Format the ARs address.
    // -------------------------------------------------------------------------------------------------------------------------
    // 05/02/05  GVandy  PR243087  Use only active verified AR addresses.
    UseCabGetNotEndedAddress();

    if (IsEmpty(local.Ar.Street1))
    {
      export.ArStatementStatus.Text8 = "NOACTADD";

      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No Active Verified Address.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    local.SpPrintWorkSet.LocationType = local.Ar.LocationType;
    local.SpPrintWorkSet.City = local.Ar.City ?? Spaces(15);
    local.SpPrintWorkSet.Country = local.Ar.Country ?? Spaces(2);
    local.SpPrintWorkSet.County = local.Ar.County ?? Spaces(2);
    local.SpPrintWorkSet.PostalCode = local.Ar.PostalCode ?? Spaces(10);
    local.SpPrintWorkSet.Province = local.Ar.Province ?? Spaces(5);
    local.SpPrintWorkSet.State = local.Ar.State ?? Spaces(2);
    local.SpPrintWorkSet.Street3 = local.Ar.Street3 ?? Spaces(25);
    local.SpPrintWorkSet.Street4 = local.Ar.Street4 ?? Spaces(25);
    local.SpPrintWorkSet.Street1 = local.Ar.Street1 ?? Spaces(25);
    local.SpPrintWorkSet.Street2 = local.Ar.Street2 ?? Spaces(25);
    local.SpPrintWorkSet.ZipCode = local.Ar.ZipCode ?? Spaces(5);
    local.SpPrintWorkSet.Zip3 = local.Ar.Zip3 ?? Spaces(3);
    local.SpPrintWorkSet.Zip4 = local.Ar.Zip4 ?? Spaces(4);
    UseSpDocFormatAddress1();

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Retrieve AR name from Adabas.
    // -------------------------------------------------------------------------------------------------------------------------
    local.CsePersonsWorkSet.Number = import.Ar.Number;
    UseEabReadCsePersonBatch();

    if (IsEmpty(local.AbendData.Type1))
    {
      // -- Successful Adabas read occurred.
    }
    else
    {
      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          // -- Unsuccessful Adabas read occurred.
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 113.  AR not found in Adabas.";
                

              break;
            case "0148":
              local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 148.  Adabas unavailable.";
                

              break;
            default:
              local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas error, response code = " +
                local.AbendData.AdabasResponseCd + ", type = " + local
                .AbendData.Type1;

              break;
          }

          break;
        case 'C':
          // -- CICS action failed.
          local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CICS error, response code = " +
            local.AbendData.CicsResponseCd;

          break;
        default:
          // -- Action failed.
          local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Unknown Adabas error, type = " +
            local.AbendData.Type1;

          break;
      }

      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      if (AsChar(local.AbendData.Type1) == 'A' && Equal
        (local.AbendData.AdabasResponseCd, "0113"))
      {
        // -- No need to abend if the AR is not found on Adabas, just log to the
        // error file.
      }
      else
      {
        // -- Any errors beside the AR not being found on Adabas should abend.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Format AR name.
    // -------------------------------------------------------------------------------------------------------------------------
    local.Name.FirstName = local.CsePersonsWorkSet.FirstName;
    local.Name.MidInitial = local.CsePersonsWorkSet.MiddleInitial;
    local.Name.LastName = local.CsePersonsWorkSet.LastName;
    UseSpDocFormatName();
    local.CsePersonsWorkSet.FormattedName = local.FieldValue.Value ?? Spaces
      (33);

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Set all the statement address and header information.  Lines 1 - 27 
    // on the sample AR statement at the top of the cab.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ArAddress.Index = -1;
    local.OfficeAddress.Index = -1;
    local.Print.Index = -1;

    do
    {
      ++local.Print.Index;
      local.Print.CheckSize();

      switch(local.Print.Index + 1)
      {
        case 1:
          // --  Line 1
          // ***  CQ33636 and CQ8056 changes to Agency name, and CSE also wants 
          // office phone removed.   6-19-12  AHockman
          // CQ51922  Hard code office name and address
          // cq65157 2/2019  uncommented code below to change back to using 
          // local office address
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + "Child Support Services" +
            "          " + local.TextWorkArea.Text12;

          break;
        case 2:
          // --  Line 2
          ++local.OfficeAddress.Index;
          local.OfficeAddress.CheckSize();

          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.OfficeAddress.Item.GlocalOfficeAddress.Value ?? "");

          break;
        case 3:
          // --  Line 3
          ++local.OfficeAddress.Index;
          local.OfficeAddress.CheckSize();

          if (IsEmpty(local.OfficeAddress.Item.GlocalOfficeAddress.Value))
          {
            ++local.OfficeAddress.Index;
            local.OfficeAddress.CheckSize();
          }

          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.OfficeAddress.Item.GlocalOfficeAddress.Value ?? "");
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 57) + "Statement Period:";

          // --  Line 2
          break;
        case 4:
          // --  Line 4
          ++local.OfficeAddress.Index;
          local.OfficeAddress.CheckSize();

          local.TextWorkArea.Text30 = import.ReportingPeriodStarting.Text10 + " to " +
            import.ReportingPeriodEndingTextWorkArea.Text10;
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.OfficeAddress.Item.GlocalOfficeAddress.Value ?? "");
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 54) + local
            .TextWorkArea.Text30;

          break;
        case 5:
          // --  Line 5
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          break;
        case 6:
          // --  Line 6
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 54) + "Payments received after";
            

          break;
        case 7:
          // --  Line 7
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + local
            .CsePersonsWorkSet.FormattedName;
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 54) + "this date will not appear";
            

          break;
        case 8:
          // --  Line 8
          ++local.ArAddress.Index;
          local.ArAddress.CheckSize();

          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.ArAddress.Item.GlocalArAddress.Value ?? "");
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 54) + "on this statement.";

          break;
        case 9:
          // --  Line 9
          ++local.ArAddress.Index;
          local.ArAddress.CheckSize();

          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          if (IsEmpty(local.ArAddress.Item.GlocalArAddress.Value))
          {
            ++local.ArAddress.Index;
            local.ArAddress.CheckSize();
          }

          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.ArAddress.Item.GlocalArAddress.Value ?? "");

          break;
        case 10:
          // --  Line 10
          ++local.ArAddress.Index;
          local.ArAddress.CheckSize();

          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.ArAddress.Item.GlocalArAddress.Value ?? "");

          break;
        case 11:
          // --  Line 11
          ++local.ArAddress.Index;
          local.ArAddress.CheckSize();

          local.Print.Update.GlocalReportDetailLine.RptDetail = "";
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 19) + (
              local.ArAddress.Item.GlocalArAddress.Value ?? "");

          break;
        case 12:
          // --  Line 12
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          if (local.ArAddress.Index + 1 < Local.ArAddressGroup.Capacity)
          {
            ++local.ArAddress.Index;
            local.ArAddress.CheckSize();

            local.Print.Update.GlocalReportDetailLine.RptDetail =
              Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 19) + (
                local.ArAddress.Item.GlocalArAddress.Value ?? "");
          }

          break;
        case 13:
          // --  Line 13
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          break;
        case 14:
          // --  Line 14
          // *** cq33636 and cq8056 Agency name change   Ahockman   6-19-12
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "                    KANSAS DEPARTMENT FOR CHILDREN AND FAMILIES";

          break;
        case 15:
          // --  Line 15
          // *** cq33636 and cq8056 Agency name change   Ahockman   6-19-12
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "                              CHILD SUPPORT SERVICES";

          break;
        case 16:
          // --  Line 16
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          break;
        case 17:
          // --  Line 17
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "     This statement is for your information only.  No action is needed from";
            

          break;
        case 18:
          // --  Line 18
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "     you at this time.  This statement may list payments that were not sent";
            

          break;
        case 19:
          // --  Line 19
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "     to you.  A payment may be kept to repay the State for public assistance";
            

          break;
        case 20:
          // --  Line 20
          // ***CQ35556  JHarden 11-21-13
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "     that you received.  Automated payment history is available 24 hours a day";
            

          break;
        case 21:
          // --  Line 21
          // ***CQ35556  JHarden 11-21-13
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "     by contacting the Kansas Child Support Call Center by phone at";
            

          // *** cq33636 and cq8056 Agency name change   Ahockman   6-19-12
          break;
        case 22:
          // --  Line 22
          // ***CQ35556  JHarden 11-21-13
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "     1-888-7-KS-CHILD (1-888-757-2445) or TTY 1-888-688-1666.";

          // *** cq33636 and cq8056 Agency name change   Ahockman   6-19-12
          break;
        case 23:
          // --  Line 23
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          break;
        case 24:
          // --  Line 24
          local.Print.Update.GlocalReportDetailLine.RptDetail = "";

          break;
        case 25:
          // --  Line 25
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "            Payor Name/                 Person ID/  Court Order/";

          break;
        case 26:
          // --  Line 26
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "            Date           Amount       Support     Amount Kept   Amount Sent";
            

          break;
        case 27:
          // --  Line 27
          local.Print.Update.GlocalReportDetailLine.RptDetail =
            "            Collected      Collected    Type        By State      To Family";
            

          break;
        default:
          break;
      }
    }
    while(local.Print.Index < 26);

    // --  The following FOR goes to last of group_import + 1, this is needed so
    // that we write the total amount collected for the very last collection
    // date on the statement.
    import.Import1.Index = 0;

    for(var limit = import.Import1.Count + 1; import.Import1.Index < limit; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      if ((!Equal(
        import.Import1.Item.GimportObligor.Number,
        local.PreviousObligor.Number) || !
        Equal(import.Import1.Item.G.CourtOrderAppliedTo,
        local.Previous.CourtOrderAppliedTo) || !
        Equal(import.Import1.Item.G.CollectionDt, local.Previous.CollectionDt)) &&
        import.Import1.Index != 0)
      {
        // -------------------------------------------------------------------------------------------------------------------------
        // -- Need to go back and insert the total amount collected for the 
        // collection date into the line containing the collection date.
        // -------------------------------------------------------------------------------------------------------------------------
        // -- Back up the approriate number of entries in the group.
        local.Print.Index = local.Print.Index + 1 - local
          .AmountCollectedOffset.Count - 1;
        local.Print.CheckSize();

        // -------------------------------------------------------------------------------------------------------------------------
        // --  Set the total amount collected for the collection date.
        // --
        // --  12/10/10  RMathews  Expanded amount displayed from 5.2 to 6.2 
        // digits
        // -------------------------------------------------------------------------------------------------------------------------
        local.TextWorkArea.Text12 =
          NumberToString((long)(local.AmountForCollDate.TotalCurrency * 100), 8,
          8);
        local.Common.Count =
          Verify(Substring(
            local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 1, 8),
          "0");
        local.TextWorkArea.Text10 = "";

        if (local.Common.Count == 1)
        {
          // -- First non-zero character is in the first postion of the whole 
          // number portion of the number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
            1, 6) + "." + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }
        else if (local.Common.Count >= 2 && local.Common.Count <= 6)
        {
          // -- First non-zero character is within the whole number portion of 
          // the number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            1, local.Common.Count - 1) + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
            local.Common.Count, 6 - local.Common.Count + 1);
          local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + "."
            + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }
        else if (local.Common.Count > 6)
        {
          // -- First non-zero character is within the decimal portion of the 
          // number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            1, 6) + "." + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }

        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 28) + local
          .TextWorkArea.Text10 + Substring
          (local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 39, 43);

        // -- Reset subscript.
        local.Print.Index = local.Print.Index + 1 + local
          .AmountCollectedOffset.Count - 1;
        local.Print.CheckSize();

        local.AmountCollectedOffset.Count = 0;
        local.AmountForCollDate.TotalCurrency = 0;
      }

      if (import.Import1.Index >= local.LastOfImportGroup.Count)
      {
        // -- We finished processing the import group.
        break;
      }

      if (local.Print.Index + 1 < Local.PrintGroup.Capacity)
      {
        ++local.Print.Index;
        local.Print.CheckSize();
      }
      else
      {
        // -- Write to error file...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Statement Exceeded Maximum Number of Lines.  Max lines = " +
          NumberToString(Local.PrintGroup.Capacity, 12, 4);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
        }

        return;
      }

      local.Print.Update.GlocalReportDetailLine.RptDetail = "";

      // --  Line 29
      if (!Equal(import.Import1.Item.GimportObligor.Number,
        local.PreviousObligor.Number) || !
        Equal(import.Import1.Item.G.CourtOrderAppliedTo,
        local.Previous.CourtOrderAppliedTo))
      {
        if (local.Print.Index + 1 < Local.PrintGroup.Capacity)
        {
          // --  Line 28 and 32
          // -- Insert a blank line.
          ++local.Print.Index;
          local.Print.CheckSize();
        }
        else
        {
          // -- Write to error file...
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Statement Exceeded Maximum Number of Lines.  Max lines = " +
            NumberToString(Local.PrintGroup.Capacity, 12, 4);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
          }

          return;
        }

        if (!Equal(import.Import1.Item.GimportObligor.Number,
          local.PreviousObligor.Number))
        {
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Retrieve Obligor name.
          // -------------------------------------------------------------------------------------------------------------------------
          local.CsePersonsWorkSet.Number =
            import.Import1.Item.GimportObligor.Number;
          UseEabReadCsePersonBatch();

          if (IsEmpty(local.AbendData.Type1))
          {
            // -- Successful Adabas read occurred.
          }
          else
          {
            switch(AsChar(local.AbendData.Type1))
            {
              case 'A':
                // -- Unsuccessful Adabas read occurred.
                switch(TrimEnd(local.AbendData.AdabasResponseCd))
                {
                  case "0113":
                    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 113.  Obligor " +
                      import.Import1.Item.GimportObligor.Number + " not found in Adabas.";
                      

                    break;
                  case "0148":
                    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas response code 148.  Obligor " +
                      import.Import1.Item.GimportObligor.Number + ".  Adabas unavailable.";
                      

                    break;
                  default:
                    local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Adabas error, response code = " +
                      local.AbendData.AdabasResponseCd + ", type = " + local
                      .AbendData.Type1 + "  Obligor " + import
                      .Import1.Item.GimportObligor.Number;

                    break;
                }

                break;
              case 'C':
                // -- CICS action failed.
                local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CICS error, response code = " +
                  local.AbendData.CicsResponseCd + "  Obligor " + import
                  .Import1.Item.GimportObligor.Number;

                break;
              default:
                // -- Action failed.
                local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Unknown Adabas error, type = " +
                  local.AbendData.Type1 + "  Obligor " + import
                  .Import1.Item.GimportObligor.Number;

                break;
            }
          }

          // -------------------------------------------------------------------------------------------------------------------------
          // --  Format Obligor name.
          // -------------------------------------------------------------------------------------------------------------------------
          local.Name.FirstName = local.CsePersonsWorkSet.FirstName;
          local.Name.MidInitial = local.CsePersonsWorkSet.MiddleInitial;
          local.Name.LastName = local.CsePersonsWorkSet.LastName;
          UseSpDocFormatName();
          local.CsePersonsWorkSet.FormattedName = local.FieldValue.Value ?? Spaces
            (33);
        }

        // --  Show court order number as the KPC standard number.
        if (CharAt(import.Import1.Item.G.CourtOrderAppliedTo, 6) == '*')
        {
          // --  Replace '*' in position 6 with space.
          local.Collection.CourtOrderAppliedTo =
            Substring(import.Import1.Item.G.CourtOrderAppliedTo, 20, 1, 5) + " " +
            Substring(import.Import1.Item.G.CourtOrderAppliedTo, 20, 7, 6);
        }
        else
        {
          local.Collection.CourtOrderAppliedTo =
            import.Import1.Item.G.CourtOrderAppliedTo ?? "";
        }

        // -------------------------------------------------------------------------------------------------------------------------
        // --  Set the Obligor Name, Obligor Person Number, Court Order Number.
        // -------------------------------------------------------------------------------------------------------------------------
        local.Print.Update.GlocalReportDetailLine.RptDetail = "            " + local
          .CsePersonsWorkSet.FormattedName;
        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 40) + local
          .CsePersonsWorkSet.Number;
        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 52) + (
            local.Collection.CourtOrderAppliedTo ?? "");

        ++local.Print.Index;
        local.Print.CheckSize();
      }

      if (Equal(import.Import1.Item.G.CollectionDt, local.Previous.CollectionDt) &&
        Equal
        (import.Import1.Item.GimportObligor.Number, local.PreviousObligor.Number)
        && Equal
        (import.Import1.Item.G.CourtOrderAppliedTo,
        local.Previous.CourtOrderAppliedTo))
      {
        ++local.AmountCollectedOffset.Count;
      }
      else
      {
        if (Equal(import.Import1.Item.GimportObligor.Number,
          local.PreviousObligor.Number) && Equal
          (import.Import1.Item.G.CourtOrderAppliedTo,
          local.Previous.CourtOrderAppliedTo))
        {
          if (local.Print.Index + 1 < Local.PrintGroup.Capacity)
          {
            // -- Skip a line.  This is a new collection date for the same 
            // obligor and court order number.
            ++local.Print.Index;
            local.Print.CheckSize();
          }
          else
          {
            // -- Write to error file...
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Statement Exceeded Maximum Number of Lines.  Max lines = " +
              NumberToString(Local.PrintGroup.Capacity, 12, 4);
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
            }

            return;
          }
        }

        local.TextWorkArea.Text10 =
          NumberToString(Month(import.Import1.Item.G.CollectionDt), 14, 2);
        local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + "/";
        local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + NumberToString
          (Day(import.Import1.Item.G.CollectionDt), 14, 2);
        local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + "/";
        local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + NumberToString
          (Year(import.Import1.Item.G.CollectionDt), 12, 4);

        // --  Line 30
        // -------------------------------------------------------------------------------------------------------------------------
        // --  Set the Collection Date.
        // -------------------------------------------------------------------------------------------------------------------------
        local.Print.Update.GlocalReportDetailLine.RptDetail = "";
        local.Print.Update.GlocalReportDetailLine.RptDetail = "            " + local
          .TextWorkArea.Text10 + "     $";
      }

      // --  Line 30 (continued) and Line 31
      if (AsChar(import.Import1.Item.G.AppliedToCode) == 'A')
      {
        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 40) + "Arrears";
      }
      else if (AsChar(import.Import1.Item.G.AppliedToCode) == 'C')
      {
        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 40) + "Current";
      }
      else
      {
        // -- Write to error file...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Unrecognized Applied to Code = " +
          import.Import1.Item.G.AppliedToCode;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
        }

        return;
      }

      // --  Line 30 (continued) and Line 31 (continued)
      if (import.Import1.Item.GimportRetained.Amount != 0)
      {
        // -------------------------------------------------------------------------------------------------------------------------
        // --  Set the amount retained by the state.
        // --
        // -- 12/10/10  RMathews   Expanded amount displayed from 5.2 to 6.2 
        // digits
        // -------------------------------------------------------------------------------------------------------------------------
        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 52) + "$";
        local.TextWorkArea.Text12 =
          NumberToString((long)(import.Import1.Item.GimportRetained.Amount * 100)
          , 8, 8);
        local.Common.Count =
          Verify(Substring(
            local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 1, 8),
          "0");
        local.TextWorkArea.Text10 = "";

        if (local.Common.Count == 1)
        {
          // -- First non-zero character is in the first postion of the whole 
          // number portion of the number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
            1, 6) + "." + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }
        else if (local.Common.Count >= 2 && local.Common.Count <= 6)
        {
          // -- First non-zero character is within the whole number portion of 
          // the number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            1, local.Common.Count - 1) + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
            local.Common.Count, 6 - local.Common.Count + 1);
          local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + "."
            + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }
        else if (local.Common.Count > 6)
        {
          // -- First non-zero character is within the decimal portion of the 
          // number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            1, 6) + "." + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }

        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
          (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 1, 9);
      }

      // --  Line 30 (continued) and Line 31 (continued)
      if (import.Import1.Item.GimportForwardedToFamily.Amount != 0)
      {
        // -------------------------------------------------------------------------------------------------------------------------
        // --  Set the amount forwarded to the family.
        // --
        // -- 12/10/10  RMathews   Expanded amount displayed from 5.2 to 6.2 
        // digits
        // -------------------------------------------------------------------------------------------------------------------------
        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 66) + "$";
        local.TextWorkArea.Text12 =
          NumberToString((long)(import.Import1.Item.GimportForwardedToFamily.
            Amount * 100), 8, 8);
        local.Common.Count =
          Verify(Substring(
            local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 1, 8),
          "0");
        local.TextWorkArea.Text10 = "";

        if (local.Common.Count == 1)
        {
          // -- First non-zero character is in the first postion of the whole 
          // number portion of the number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
            1, 6) + "." + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }
        else if (local.Common.Count >= 2 && local.Common.Count <= 6)
        {
          // -- First non-zero character is within the whole number portion of 
          // the number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            1, local.Common.Count - 1) + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
            local.Common.Count, 6 - local.Common.Count + 1);
          local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + "."
            + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }
        else if (local.Common.Count > 6)
        {
          // -- First non-zero character is within the decimal portion of the 
          // number.
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            1, 6) + "." + Substring
            (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
        }

        local.Print.Update.GlocalReportDetailLine.RptDetail =
          Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 67) + Substring
          (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 1, 9);
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // -- Keep running totals for the amount collected on the collection date 
      // and the total amount sent to family.
      // -------------------------------------------------------------------------------------------------------------------------
      local.AmountForCollDate.TotalCurrency =
        local.AmountForCollDate.TotalCurrency + import
        .Import1.Item.GimportRetained.Amount + import
        .Import1.Item.GimportForwardedToFamily.Amount;
      local.TotalAmountToFamily.TotalCurrency += import.Import1.Item.
        GimportForwardedToFamily.Amount;

      // -- Move current views to previous views.
      local.Previous.Assign(import.Import1.Item.G);
      local.PreviousObligor.Number = import.Import1.Item.GimportObligor.Number;
    }

    import.Import1.CheckIndex();

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Write total amount sent to family.
    // -------------------------------------------------------------------------------------------------------------------------
    if (local.Print.Index + 1 < Local.PrintGroup.Capacity - 3)
    {
      // -- Continue.
    }
    else
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Statement Exceeded Maximum Number of Lines.  Max lines = " +
        NumberToString(Local.PrintGroup.Capacity, 12, 4);
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    // --  Line 36, 37, 38
    // -- Skip 2 lines.
    local.Print.Index += 2;
    local.Print.CheckSize();

    local.Print.Update.GlocalReportDetailLine.RptDetail = "";
    local.Print.Update.GlocalReportDetailLine.RptDetail =
      Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 66) + "----------";

    // --  Line 39
    // --
    // --  12/10/10  RMathews  Expanded amount field from 5.2 to 6.2 digits
    ++local.Print.Index;
    local.Print.CheckSize();

    local.Print.Update.GlocalReportDetailLine.RptDetail = "";
    local.Print.Update.GlocalReportDetailLine.RptDetail =
      Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 31) + "Total sent to the family:          $";
      

    if (local.TotalAmountToFamily.TotalCurrency == 0)
    {
      local.TextWorkArea.Text10 = "     0.00";
    }
    else if (local.TotalAmountToFamily.TotalCurrency > 999999.99M)
    {
      local.TextWorkArea.Text10 = "*********";
    }
    else
    {
      local.TextWorkArea.Text12 =
        NumberToString((long)(local.TotalAmountToFamily.TotalCurrency * 100), 8,
        8);
      local.Common.Count =
        Verify(Substring(
          local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 1, 8), "0");
        
      local.TextWorkArea.Text10 = "";

      if (local.Common.Count == 1)
      {
        // -- First non-zero character is in the first postion of the whole 
        // number portion of the number.
        local.TextWorkArea.Text10 =
          Substring(local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 1,
          6) + "." + Substring
          (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
      }
      else if (local.Common.Count >= 2 && local.Common.Count <= 6)
      {
        // -- First non-zero character is within the whole number portion of the
        // number.
        local.TextWorkArea.Text10 =
          Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 1,
          local.Common.Count - 1) + Substring
          (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength,
          local.Common.Count, 6 - local.Common.Count + 1);
        local.TextWorkArea.Text10 = TrimEnd(local.TextWorkArea.Text10) + "." + Substring
          (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
      }
      else if (local.Common.Count > 6)
      {
        // -- First non-zero character is within the decimal portion of the 
        // number.
        local.TextWorkArea.Text10 =
          Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 1,
          6) + "." + Substring
          (local.TextWorkArea.Text12, TextWorkArea.Text12_MaxLength, 7, 2);
      }
    }

    local.Print.Update.GlocalReportDetailLine.RptDetail =
      Substring(local.Print.Item.GlocalReportDetailLine.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 67) + local.TextWorkArea.Text10;

    // --  Line 57,58,59
    // -------------------------------------------------------------------------------------------------------------------------
    // -- Write disclaimer in the footer of the last page.
    // -------------------------------------------------------------------------------------------------------------------------
    local.NumberOfPages.Count = (local.Print.Index + 5) / local
      .NumberOfLinesPerPage.Count;
    local.Remainder.Count = (int)(local.Print.Index + 5 - (
      long)local.NumberOfPages.Count * local.NumberOfLinesPerPage.Count);

    if (local.Remainder.Count > 0)
    {
      // -- Increment the page count to account for a partial page of data.
      ++local.NumberOfPages.Count;
    }

    // 05/17/05  GVandy  PR244676  'This page left blank intentionally' not 
    // displaying in one situation.
    if (local.Print.Index + 1 <= ((long)local.NumberOfPages.Count - 1) * (
      decimal)local.NumberOfLinesPerPage.Count)
    {
      // 05/02/05  GVandy  PR243273  Add message 'This page left blank 
      // intentionally' if the only info on the last page is the footer
      // disclaimer.
      local.Print.Index = (int)((local.NumberOfPages.Count - 1) * (
        long)local.NumberOfLinesPerPage.Count + (
          local.NumberOfLinesPerPage.Count - 3) / 2 - 1);
      local.Print.CheckSize();

      local.Print.Update.GlocalReportDetailLine.RptDetail =
        "                    This page intentionally left blank.";
    }

    if ((long)local.NumberOfLinesPerPage.Count * local.NumberOfPages.Count > Local
      .PrintGroup.Capacity)
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Statement Exceeded Maximum Number of Lines.  Max lines = " +
        NumberToString(Local.PrintGroup.Capacity, 12, 4);
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }

      return;
    }

    local.SubscriptForFooter.Count =
      (int)((long)local.NumberOfLinesPerPage.Count * local
      .NumberOfPages.Count - 2);

    local.Print.Index = local.SubscriptForFooter.Count - 1;
    local.Print.CheckSize();

    local.Print.Update.GlocalReportDetailLine.RptDetail =
      "     Due to fees and other adjustments, the amount received may be less";
      

    ++local.Print.Index;
    local.Print.CheckSize();

    local.Print.Update.GlocalReportDetailLine.RptDetail =
      "     than what is shown.  Support Type: Current - support due in current";
      

    ++local.Print.Index;
    local.Print.CheckSize();

    local.Print.Update.GlocalReportDetailLine.RptDetail =
      "     month, Arrears - support past due.";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Set marks for the mailing machine.
    // -------------------------------------------------------------------------------------------------------------------------
    if (local.NumberOfPages.Count > 3)
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Mailer Machine - INTEGRITY PAGE 4
      // -------------------------------------------------------------------------------------------------------------------------
      local.Print.Index = 0;
      local.Print.CheckSize();

      local.Print.Update.GlocalReportDetailLine.RptDetail = "     ____" + Substring
        (local.Print.Item.GlocalReportDetailLine.RptDetail,
        EabReportSend.RptDetail_MaxLength, 10, 120);
    }

    if (local.NumberOfPages.Count == 2 || local.NumberOfPages.Count == 3 || local
      .NumberOfPages.Count == 6 || local.NumberOfPages.Count == 7)
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Mailer Machine - INTEGRITY PAGE 2
      // -------------------------------------------------------------------------------------------------------------------------
      local.Print.Index = 1;
      local.Print.CheckSize();

      local.Print.Update.GlocalReportDetailLine.RptDetail = "     ____" + Substring
        (local.Print.Item.GlocalReportDetailLine.RptDetail,
        EabReportSend.RptDetail_MaxLength, 10, 120);
    }

    if (local.NumberOfPages.Count == 1 || local.NumberOfPages.Count == 3 || local
      .NumberOfPages.Count == 5 || local.NumberOfPages.Count == 7)
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Mailer Machine - INTEGRITY PAGE 1
      // -------------------------------------------------------------------------------------------------------------------------
      local.Print.Index = 2;
      local.Print.CheckSize();

      local.Print.Update.GlocalReportDetailLine.RptDetail = "     ____" + Substring
        (local.Print.Item.GlocalReportDetailLine.RptDetail,
        EabReportSend.RptDetail_MaxLength, 10, 120);
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Mailer Machine - END COLLATION
    // -------------------------------------------------------------------------------------------------------------------------
    local.Print.Index = 6;
    local.Print.CheckSize();

    local.Print.Update.GlocalReportDetailLine.RptDetail = "     ____" + Substring
      (local.Print.Item.GlocalReportDetailLine.RptDetail,
      EabReportSend.RptDetail_MaxLength, 10, 120);
    local.Print.Index = 7;

    for(var increment = local.NumberOfLinesPerPage.Count; increment >= 0
      ? local.Print.Index < local.Print.Count : local.Print.Index + 1 >= local
      .Print.Count; local.Print.Index = local.Print.Index + 1 + increment - 1)
    {
      if (!local.Print.CheckSize())
      {
        break;
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Mailer Machine - BENCHMARK
      // -------------------------------------------------------------------------------------------------------------------------
      local.Print.Update.GlocalReportDetailLine.RptDetail = "     ____" + Substring
        (local.Print.Item.GlocalReportDetailLine.RptDetail,
        EabReportSend.RptDetail_MaxLength, 10, 120);
    }

    local.Print.CheckIndex();

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write the AR statement to the appropriate report.
    // --
    // --  The reports are numbered 1 through 9.  Report 1 contains all 
    // statements that are only 1 page in length, report 2
    // --  contains statements that are 2 pages in length, etc.  The exception 
    // being that report nine contains all reports
    // --  that are 9 or more pages in length.
    // -------------------------------------------------------------------------------------------------------------------------
    if (local.NumberOfPages.Count <= 8)
    {
      local.EabReportSend.ReportNumber = local.NumberOfPages.Count;

      // -- Set the subscript of the statement count group to the appropriate 
      // entry.
      import.GimportExportStatementCount.Index = local.NumberOfPages.Count - 1;
      import.GimportExportStatementCount.CheckSize();
    }
    else
    {
      local.EabReportSend.ReportNumber = 9;

      // -- Set the subscript of the statement count group to the appropriate 
      // entry.
      import.GimportExportStatementCount.Index = 8;
      import.GimportExportStatementCount.CheckSize();
    }

    local.EabFileHandling.Action = "WRITE";

    if (import.GimportExportStatementCount.Item.GimportExportCount.Count == 0)
    {
      // -- If this is the first statement written to the report, then write a 
      // header page that identifies what the report contains.
      local.EabReportSend.Command = "DETAIL";

      for(local.Common.Count = 1; local.Common.Count <= 8; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "******************************************************************************";
              

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "**                                                                          **";
              

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "**         This report contains AR Statements for Reporting Period          **";
              

            break;
          case 4:
            local.EabReportSend.RptDetail = "**                         " + import
              .ReportingPeriodStarting.Text10 + " to " + import
              .ReportingPeriodEndingTextWorkArea.Text10 + "                         **";
              

            break;
          case 5:
            local.EabReportSend.RptDetail =
              "**                                                                          **";
              

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "**                 These statements are " + NumberToString
              (local.EabReportSend.ReportNumber, 15, 1) + " page(s) in length.                **";
              

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "**                                                                          **";
              

            break;
          case 8:
            local.EabReportSend.RptDetail =
              "******************************************************************************";
              

            break;
          default:
            break;
        }

        UseSpEabWriteDocument();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          // -- Write to error file...
          local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Error Writing Report " +
            NumberToString(local.EabReportSend.ReportNumber, 15, 1) + " Header.  Return Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
          }

          return;
        }
      }
    }

    local.Common.Count = 0;
    local.Print.Index = 0;

    for(var limit = local.Print.Count; local.Print.Index < limit; ++
      local.Print.Index)
    {
      if (!local.Print.CheckSize())
      {
        break;
      }

      ++local.Common.Count;

      // -- Determine the action to send to the eab report writer. (i.e. do we 
      // need a page break)
      if (local.Print.Index == 0)
      {
        local.EabReportSend.Command = "NEWPAGE";
      }
      else if (local.Common.Count <= local.NumberOfLinesPerPage.Count)
      {
        local.EabReportSend.Command = "DETAIL";
      }
      else
      {
        local.EabReportSend.Command = "NEWPAGE";
        local.Common.Count = 1;
      }

      local.EabReportSend.RptDetail =
        local.Print.Item.GlocalReportDetailLine.RptDetail;
      UseSpEabWriteDocument();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        // -- Write to error file...
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - Error Writing AR Statement to Report Number " +
          NumberToString(local.EabReportSend.ReportNumber, 15, 1) + ".  Return Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
        }

        return;
      }
    }

    local.Print.CheckIndex();

    // -- Reset return status to indicate the statement printed.
    export.ArStatementStatus.Text8 = "PRINTED";

    // -- Increment number of statements count.
    import.GimportExportStatementCount.Update.GimportExportCount.Count =
      import.GimportExportStatementCount.Item.GimportExportCount.Count + 1;

    if (AsChar(import.CreateEvents.Flag) == 'Y')
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Create HIST record indicating AR statement was generated.
      // --
      // --  Event id = 19
      // --  Reason Code = ARSTMNT
      // --
      // -------------------------------------------------------------------------------------------------------------------------
      local.Infrastructure.SituationNumber = 1;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 19;
      local.Infrastructure.ReasonCode = "ARSTMNT";
      local.Infrastructure.BusinessObjectCd = "DOC";
      local.Infrastructure.InitiatingStateCode = "KS";
      local.Infrastructure.CsePersonNumber = import.Ar.Number;
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.Detail = "Document generated in batch.";
      UseSpCabCreateInfrastructure();
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1ToArAddress(SpDocFormatAddress.Export.
    ExportGroup source, Local.ArAddressGroup target)
  {
    target.GlocalArAddress.Value = source.G.Value;
  }

  private static void MoveExport1ToOfficeAddress(SpDocFormatAddress.Export.
    ExportGroup source, Local.OfficeAddressGroup target)
  {
    target.GlocalOfficeAddress.Value = source.G.Value;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabGetNotEndedAddress()
  {
    var useImport = new CabGetNotEndedAddress.Import();
    var useExport = new CabGetNotEndedAddress.Export();

    useImport.CsePerson.Number = import.Ar.Number;

    Call(CabGetNotEndedAddress.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Ar);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.AbendData.Assign(local.AbendData);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseFnB664DetermineAr()
  {
    var useImport = new FnB664DetermineAr.Import();
    var useExport = new FnB664DetermineAr.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.Debt);
    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.ReportingPeriodEnding.Date =
      import.ReportingPeriodEndingDateWorkArea.Date;
    useImport.Voluntary.SystemGeneratedIdentifier =
      import.Voluntary.SystemGeneratedIdentifier;
    useImport.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      import.SpousalArrearsJudgement.SystemGeneratedIdentifier;
    useImport.SpousalSupport.SystemGeneratedIdentifier =
      import.SpousalSupport.SystemGeneratedIdentifier;
    useImport.DueDate.Date = local.DueDate.Date;

    Call(FnB664DetermineAr.Execute, useImport, useExport);

    MoveCsePerson(useExport.Ar, local.DerivedAr);
  }

  private void UseFnDeterminePgmForDebtDetail1()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.Supported2.Number;
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.DebtDetail.Assign(entities.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private void UseFnDeterminePgmForDebtDetail2()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.Supported2.Number;
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.DebtDetail.Assign(local.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpDocFormatAddress1()
  {
    var useImport = new SpDocFormatAddress.Import();
    var useExport = new SpDocFormatAddress.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatAddress.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.ArAddress, MoveExport1ToArAddress);
  }

  private void UseSpDocFormatAddress2()
  {
    var useImport = new SpDocFormatAddress.Import();
    var useExport = new SpDocFormatAddress.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatAddress.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.OfficeAddress, MoveExport1ToOfficeAddress);
  }

  private void UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    useImport.SpPrintWorkSet.Assign(local.Name);

    Call(SpDocFormatName.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.FieldValue.Value;
  }

  private void UseSpEabWriteDocument()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.Assign(local.EabReportSend);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.LastAccrualDt =
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
        db.SetString(command, "cspNumber", entities.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Obligor1.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor1.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Populated = true;
      });
  }

  private bool ReadDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt1",
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

  private bool ReadDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "obTrnTyp", entities.DebtDetail.OtrType);
        db.SetInt32(command, "obTrnId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
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

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadDebtDetail2()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail2",
      (db, command) =>
      {
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
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Debt.OtyType);
        db.SetInt32(command, "obId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadOfficeOfficeAddress()
  {
    entities.Office.Populated = false;
    entities.OfficeAddress1.Populated = false;

    return Read("ReadOfficeOfficeAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress1.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.OfficeAddress1.Type1 = db.GetString(reader, 5);
        entities.OfficeAddress1.Street1 = db.GetString(reader, 6);
        entities.OfficeAddress1.Street2 = db.GetNullableString(reader, 7);
        entities.OfficeAddress1.City = db.GetString(reader, 8);
        entities.OfficeAddress1.StateProvince = db.GetString(reader, 9);
        entities.OfficeAddress1.Zip = db.GetNullableString(reader, 10);
        entities.OfficeAddress1.Zip4 = db.GetNullableString(reader, 11);
        entities.OfficeAddress1.Zip3 = db.GetNullableString(reader, 12);
        entities.Office.Populated = true;
        entities.OfficeAddress1.Populated = true;
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
    /// <summary>A GimportExportStatementCountGroup group.</summary>
    [Serializable]
    public class GimportExportStatementCountGroup
    {
      /// <summary>
      /// A value of GimportExportCount.
      /// </summary>
      [JsonPropertyName("gimportExportCount")]
      public Common GimportExportCount
      {
        get => gimportExportCount ??= new();
        set => gimportExportCount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common gimportExportCount;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of GimportObligor.
      /// </summary>
      [JsonPropertyName("gimportObligor")]
      public CsePerson GimportObligor
      {
        get => gimportObligor ??= new();
        set => gimportObligor = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Collection G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimportRetained.
      /// </summary>
      [JsonPropertyName("gimportRetained")]
      public Collection GimportRetained
      {
        get => gimportRetained ??= new();
        set => gimportRetained = value;
      }

      /// <summary>
      /// A value of GimportForwardedToFamily.
      /// </summary>
      [JsonPropertyName("gimportForwardedToFamily")]
      public Collection GimportForwardedToFamily
      {
        get => gimportForwardedToFamily ??= new();
        set => gimportForwardedToFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson gimportObligor;
      private Collection g;
      private Collection gimportRetained;
      private Collection gimportForwardedToFamily;
    }

    /// <summary>
    /// A value of Non718BCollection.
    /// </summary>
    [JsonPropertyName("non718BCollection")]
    public Common Non718BCollection
    {
      get => non718BCollection ??= new();
      set => non718BCollection = value;
    }

    /// <summary>
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingDateWorkArea")]
    public DateWorkArea ReportingPeriodEndingDateWorkArea
    {
      get => reportingPeriodEndingDateWorkArea ??= new();
      set => reportingPeriodEndingDateWorkArea = value;
    }

    /// <summary>
    /// Gets a value of GimportExportStatementCount.
    /// </summary>
    [JsonIgnore]
    public Array<GimportExportStatementCountGroup>
      GimportExportStatementCount => gimportExportStatementCount ??= new(
        GimportExportStatementCountGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of GimportExportStatementCount for json serialization.
    /// </summary>
    [JsonPropertyName("gimportExportStatementCount")]
    [Computed]
    public IList<GimportExportStatementCountGroup>
      GimportExportStatementCount_Json
    {
      get => gimportExportStatementCount;
      set => GimportExportStatementCount.Assign(value);
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public TextWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingTextWorkArea")]
    public TextWorkArea ReportingPeriodEndingTextWorkArea
    {
      get => reportingPeriodEndingTextWorkArea ??= new();
      set => reportingPeriodEndingTextWorkArea = value;
    }

    /// <summary>
    /// A value of Voluntary.
    /// </summary>
    [JsonPropertyName("voluntary")]
    public ObligationType Voluntary
    {
      get => voluntary ??= new();
      set => voluntary = value;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of SpousalSupport.
    /// </summary>
    [JsonPropertyName("spousalSupport")]
    public ObligationType SpousalSupport
    {
      get => spousalSupport ??= new();
      set => spousalSupport = value;
    }

    private Common non718BCollection;
    private Common createEvents;
    private DateWorkArea reportingPeriodEndingDateWorkArea;
    private Array<GimportExportStatementCountGroup> gimportExportStatementCount;
    private CsePerson ar;
    private Array<ImportGroup> import1;
    private ProgramProcessingInfo programProcessingInfo;
    private TextWorkArea reportingPeriodStarting;
    private TextWorkArea reportingPeriodEndingTextWorkArea;
    private ObligationType voluntary;
    private ObligationType spousalArrearsJudgement;
    private ObligationType spousalSupport;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GexportObligor.
      /// </summary>
      [JsonPropertyName("gexportObligor")]
      public CsePerson GexportObligor
      {
        get => gexportObligor ??= new();
        set => gexportObligor = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Collection G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportRetained.
      /// </summary>
      [JsonPropertyName("gexportRetained")]
      public Collection GexportRetained
      {
        get => gexportRetained ??= new();
        set => gexportRetained = value;
      }

      /// <summary>
      /// A value of GexportForwardedToFamily.
      /// </summary>
      [JsonPropertyName("gexportForwardedToFamily")]
      public Collection GexportForwardedToFamily
      {
        get => gexportForwardedToFamily ??= new();
        set => gexportForwardedToFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson gexportObligor;
      private Collection g;
      private Collection gexportRetained;
      private Collection gexportForwardedToFamily;
    }

    /// <summary>
    /// A value of ArStatementStatus.
    /// </summary>
    [JsonPropertyName("arStatementStatus")]
    public TextWorkArea ArStatementStatus
    {
      get => arStatementStatus ??= new();
      set => arStatementStatus = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private TextWorkArea arStatementStatus;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ArAddressGroup group.</summary>
    [Serializable]
    public class ArAddressGroup
    {
      /// <summary>
      /// A value of GlocalArAddress.
      /// </summary>
      [JsonPropertyName("glocalArAddress")]
      public FieldValue GlocalArAddress
      {
        get => glocalArAddress ??= new();
        set => glocalArAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalArAddress;
    }

    /// <summary>A PrintGroup group.</summary>
    [Serializable]
    public class PrintGroup
    {
      /// <summary>
      /// A value of GlocalReportDetailLine.
      /// </summary>
      [JsonPropertyName("glocalReportDetailLine")]
      public EabReportSend GlocalReportDetailLine
      {
        get => glocalReportDetailLine ??= new();
        set => glocalReportDetailLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private EabReportSend glocalReportDetailLine;
    }

    /// <summary>A OfficeAddressGroup group.</summary>
    [Serializable]
    public class OfficeAddressGroup
    {
      /// <summary>
      /// A value of GlocalOfficeAddress.
      /// </summary>
      [JsonPropertyName("glocalOfficeAddress")]
      public FieldValue GlocalOfficeAddress
      {
        get => glocalOfficeAddress ??= new();
        set => glocalOfficeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalOfficeAddress;
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
    /// A value of DerivedAr.
    /// </summary>
    [JsonPropertyName("derivedAr")]
    public CsePerson DerivedAr
    {
      get => derivedAr ??= new();
      set => derivedAr = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
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
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
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
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public SpPrintWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of LastOfImportGroup.
    /// </summary>
    [JsonPropertyName("lastOfImportGroup")]
    public Common LastOfImportGroup
    {
      get => lastOfImportGroup ??= new();
      set => lastOfImportGroup = value;
    }

    /// <summary>
    /// A value of SubscriptForFooter.
    /// </summary>
    [JsonPropertyName("subscriptForFooter")]
    public Common SubscriptForFooter
    {
      get => subscriptForFooter ??= new();
      set => subscriptForFooter = value;
    }

    /// <summary>
    /// A value of Remainder.
    /// </summary>
    [JsonPropertyName("remainder")]
    public Common Remainder
    {
      get => remainder ??= new();
      set => remainder = value;
    }

    /// <summary>
    /// A value of NumberOfPages.
    /// </summary>
    [JsonPropertyName("numberOfPages")]
    public Common NumberOfPages
    {
      get => numberOfPages ??= new();
      set => numberOfPages = value;
    }

    /// <summary>
    /// A value of NumberOfLinesPerPage.
    /// </summary>
    [JsonPropertyName("numberOfLinesPerPage")]
    public Common NumberOfLinesPerPage
    {
      get => numberOfLinesPerPage ??= new();
      set => numberOfLinesPerPage = value;
    }

    /// <summary>
    /// A value of TotalAmountToFamily.
    /// </summary>
    [JsonPropertyName("totalAmountToFamily")]
    public Common TotalAmountToFamily
    {
      get => totalAmountToFamily ??= new();
      set => totalAmountToFamily = value;
    }

    /// <summary>
    /// A value of AmountForCollDate.
    /// </summary>
    [JsonPropertyName("amountForCollDate")]
    public Common AmountForCollDate
    {
      get => amountForCollDate ??= new();
      set => amountForCollDate = value;
    }

    /// <summary>
    /// A value of AmountCollectedOffset.
    /// </summary>
    [JsonPropertyName("amountCollectedOffset")]
    public Common AmountCollectedOffset
    {
      get => amountCollectedOffset ??= new();
      set => amountCollectedOffset = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// Gets a value of ArAddress.
    /// </summary>
    [JsonIgnore]
    public Array<ArAddressGroup> ArAddress => arAddress ??= new(
      ArAddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ArAddress for json serialization.
    /// </summary>
    [JsonPropertyName("arAddress")]
    [Computed]
    public IList<ArAddressGroup> ArAddress_Json
    {
      get => arAddress;
      set => ArAddress.Assign(value);
    }

    /// <summary>
    /// Gets a value of Print.
    /// </summary>
    [JsonIgnore]
    public Array<PrintGroup> Print => print ??= new(PrintGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Print for json serialization.
    /// </summary>
    [JsonPropertyName("print")]
    [Computed]
    public IList<PrintGroup> Print_Json
    {
      get => print;
      set => Print.Assign(value);
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonAddress Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// Gets a value of OfficeAddress.
    /// </summary>
    [JsonIgnore]
    public Array<OfficeAddressGroup> OfficeAddress => officeAddress ??= new(
      OfficeAddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OfficeAddress for json serialization.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    [Computed]
    public IList<OfficeAddressGroup> OfficeAddress_Json
    {
      get => officeAddress;
      set => OfficeAddress.Assign(value);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PreviousAr.
    /// </summary>
    [JsonPropertyName("previousAr")]
    public CsePerson PreviousAr
    {
      get => previousAr ??= new();
      set => previousAr = value;
    }

    /// <summary>
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Collection Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private DateWorkArea null1;
    private CsePerson derivedAr;
    private DateWorkArea dueDate;
    private Infrastructure infrastructure;
    private Common caseFound;
    private DebtDetail debtDetail;
    private DprProgram dprProgram;
    private Program program;
    private SpPrintWorkSet name;
    private FieldValue fieldValue;
    private Common lastOfImportGroup;
    private Common subscriptForFooter;
    private Common remainder;
    private Common numberOfPages;
    private Common numberOfLinesPerPage;
    private Common totalAmountToFamily;
    private Common amountForCollDate;
    private Common amountCollectedOffset;
    private Common common;
    private Collection collection;
    private TextWorkArea textWorkArea;
    private Array<ArAddressGroup> arAddress;
    private Array<PrintGroup> print;
    private CsePersonAddress ar;
    private SpPrintWorkSet spPrintWorkSet;
    private Array<OfficeAddressGroup> officeAddress;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson previousAr;
    private CsePerson previousObligor;
    private Collection previous;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress1.
    /// </summary>
    [JsonPropertyName("officeAddress1")]
    public OfficeAddress OfficeAddress1
    {
      get => officeAddress1 ??= new();
      set => officeAddress1 = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private CsePersonAccount supported1;
    private ObligationType obligationType;
    private CsePerson supported2;
    private ObligationTransaction debt;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Obligation obligation;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePerson ar;
    private Case1 case1;
    private CaseRole caseRole;
    private Office office;
    private OfficeAddress officeAddress1;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
  }
#endregion
}
