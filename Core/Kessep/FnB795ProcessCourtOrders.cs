// Program: FN_B795_PROCESS_COURT_ORDERS, ID: 1902457337, model: 746.
// Short name: SWE03736
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_PROCESS_COURT_ORDERS.
/// </summary>
[Serializable]
public partial class FnB795ProcessCourtOrders: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_PROCESS_COURT_ORDERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795ProcessCourtOrders(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795ProcessCourtOrders.
  /// </summary>
  public FnB795ProcessCourtOrders(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------
    // --  C O U R T    O R D E R    S P E C I F I C    D A T A    E L E M E N T
    // S   (TYPE B RECORD)   --
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Assumptions:
    // 	5) For all debt/payment information, the tie from court order to case 
    // will be made
    // 	   through LROL:
    // 		a. NCP is active on the case
    // 		b. NCP is listed on LROL with an AP Role with the Case # for J and O 
    // court
    // 		   order legal action
    // --------------------------------------------------------------------------------------------------
    local.NullTextDate.Text10 = "01-01-0001";
    local.Common.TotalCurrency = 0M;
    local.NullCurrency.Text15 = UseFnB795ConvertNumToText();
    local.Ap.Type1 = "AP";
    local.I.Classification = "I";
    local.J.Classification = "J";
    local.M.Classification = "M";
    local.O.Classification = "O";
    local.P.Classification = "P";
    local.Wa.Code = "WA";
    local.F1.Code = "F";
    local.F1.SequentialIdentifier = 3;
    local.S.Code = "S";
    local.S.SequentialIdentifier = 4;
    export.Group.Index = -1;
    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";

    foreach(var item in ReadLegalAction9())
    {
      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.G.Assign(import.NullCourtOrder);

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Court Order
      // Report standard number of associated court orders (see Assumptions #5 
      // for debt/collections, and IWO/Petition/Contempt rules below).  This may
      // be left blank if the only action is a petition created with no court
      // order number entered yet.
      // --------------------------------------------------------------------------------------------------
      export.Group.Update.G.CoCourtOrderNumber =
        Substring(entities.LegalAction.StandardNumber, 1, 12);

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Current Support Collected in Month
      // This is the Dollar Amount of Current Support Collected during the 
      // report month.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // combination,
      // 	   report the sum Collection_Amt where the Dashboard_Priority = 1-4(N
      // )IM.
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      local.DashboardAuditData.DashboardPriority = "1-4(N)IM";
      ReadDashboardAuditData3();
      export.Group.Update.G.CoCsCollectedInMonth = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Current Support Due in Month
      // This is the Dollar Amount of Current Support Due during the report 
      // month.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // combination,
      // 	   report the sum (Collection_Amt + Debt_Balance_Due) where the
      // 	   Dashboard_Priority = 1-4(D)IM.  Need both because it counts 
      // collections applied
      // 	   to gifts.
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      local.DashboardAuditData.DashboardPriority = "1-4(D)IM";
      ReadDashboardAuditData1();
      export.Group.Update.G.CoCsDueInMonth = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Current Support Collected FFYTD
      // This is the Dollar Amount of Current Support Collected during the 
      // federal fiscal year.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // combination,
      // 	   report the sum Collection_Amt where the Dashboard_Priority = 1-4(N
      // )FY.
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      local.DashboardAuditData.DashboardPriority = "1-4(N)FY";
      ReadDashboardAuditData3();
      export.Group.Update.G.CoCsCollectedFfytd = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Current Support Due FFYTD
      // This is the Dollar Amount of Current Support Due during the federal 
      // fiscal year.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // combination,
      // 	   report the sum (Collection_Amt + Debt_Balance_Due) where the
      // 	   Dashboard_Priority = 1-4(D)FY.
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      local.DashboardAuditData.DashboardPriority = "1-4(D)FY";
      ReadDashboardAuditData1();
      export.Group.Update.G.CoCsDueFfytd = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Total Arrears Amount Due
      // This is the dollar amount of the arrears due as of the last day of the 
      // report period.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // combination,
      // 	   report the sum  (Collection_Amt + Debt_Balance_Due) where the
      // 	   Dashboard Priority = one of the following:
      // 		a. 1-8(#1A)
      // 		b. 1-8(#1B)
      // 		c. 1-8(#1C)
      // 		d. 1-8(#2A)
      // 		e. 1-8(#2B)
      // 		f. 1-8(#3A)
      // 		g. 1-8(#3B)
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      ReadDashboardAuditData2();
      export.Group.Update.G.CoTotalArrearsAmountDue =
        UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Arrears Paid in Month
      // This is the dollar amount of the collections distributed as arrears 
      // during the report month.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // comination,
      // 	   report the sum Collection_Amt where the Dashboard_Priority = 1-7IM
      // .
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      local.DashboardAuditData.DashboardPriority = "1-7IM";
      ReadDashboardAuditData3();
      export.Group.Update.G.CoArrearsPaidInMonth = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Arrears Paid FFYTD
      // This is the dollar amount of the collections distributed as arrears 
      // during the federal fiscal year.
      // 	1) From DB_AUDIT_DATA.  For each Standard_Number/ Payor_CSP_Number 
      // comination,
      // 	   report the sum Collection_Amt where the Dashboard_Priority = 1-7FY
      // .
      // 	2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      local.DashboardAuditData.DashboardPriority = "1-7FY";
      ReadDashboardAuditData3();
      export.Group.Update.G.CoArrearsPaidFfytd = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Last Court Order Specific Payment Amount (Non DSO Offset 
      // Types)
      // 1) As of the report run date.  Report the dollar amount of the most 
      // recent NCP collection
      //    according to the court order on the cash receipt detail.  Amounts do
      // not have to be
      //    distributed.
      // 2) Report as 0.00 if none.
      // --------------------------------------------------------------------------------------------------
      ReadCashReceiptDetailCollectionType1();

      if (entities.CashReceiptDetail.Populated)
      {
        local.Common.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount;
      }
      else
      {
        local.Common.TotalCurrency = 0M;
      }

      export.Group.Update.G.CoLastPaymentAmount = UseFnB795ConvertNumToText();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Last Court Order Specific Payment Date (Non DSO Offset 
      // Types)
      // 1) As of the report run date.  Report the date of the payment reported 
      // as the last court
      //    order specific NCP payment amount (non DSO Offset Types)
      // 2) Report as 01-01-0001 if none.
      // --------------------------------------------------------------------------------------------------
      if (entities.CashReceiptDetail.Populated)
      {
        local.DateWorkArea.Date = entities.CashReceiptDetail.CollectionDate;
        export.Group.Update.G.CoLastPaymentDate = UseCabFormatDate();
      }
      else
      {
        export.Group.Update.G.CoLastPaymentDate = local.NullTextDate.Text10;
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Last Court Order Specific Payment Type (Non DSO Offset 
      // Types)
      // 1) As of the report run date.  Report the type of payment reported as 
      // the last court order
      //    specific NCP payment amount (non DSO Offset Types)
      // 2) Report as blank if none.
      // --------------------------------------------------------------------------------------------------
      if (entities.CashReceiptDetail.Populated)
      {
        export.Group.Update.G.CoLastPaymentType = entities.CollectionType.Code;
      }
      else
      {
        export.Group.Update.G.CoLastPaymentType = "";
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Last DSO Offset Payment Date (DSO Offset Types)
      // 1) As of the report run date. Report the date of the payment reported 
      // as the last DSO NCP
      //    payment amount.
      // 2) Report as 01-01-0001 if none.
      // --------------------------------------------------------------------------------------------------
      ReadCashReceiptDetailCollectionType2();

      if (entities.CashReceiptDetail.Populated)
      {
        local.DateWorkArea.Date = entities.CashReceiptDetail.CollectionDate;
        export.Group.Update.G.CoLastDsoPaymentDate = UseCabFormatDate();
      }
      else
      {
        export.Group.Update.G.CoLastDsoPaymentDate = local.NullTextDate.Text10;
      }
    }

    // --------------------------------------------------------------------------------------------------
    // For the following IWO data elements:  These elements are court order 
    // specific.  For multi-payor court orders, these elements could report on
    // each NCP.  The tie from court order to case is:
    // 	NCP is active on the case
    // 	NCP is listed on LROL with an AP Role with the Case # for the qualifying
    // 	I class legal action
    // --------------------------------------------------------------------------------------------------
    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";

    foreach(var item in ReadLegalAction11())
    {
      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      // -- Do not include the court order if this is not the most recently 
      // created I class legal action.
      if (ReadLegalAction4())
      {
        continue;
      }

      // -- Check if this standard number is already in the export group.
      local.CourtOrderFound.Flag = "N";

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.G.CoCourtOrderNumber,
          entities.LegalAction.StandardNumber, 1, 12))
        {
          local.CourtOrderFound.Flag = "Y";

          break;
        }
      }

      export.Group.CheckIndex();

      if (AsChar(local.CourtOrderFound.Flag) == 'Y')
      {
        // -- The standard number was already in the export group.  Leave the 
        // subscript alone,
        //    this is where we'll put the IWO info.
      }
      else
      {
        // -- The standard number was not already in the export group.  Create a
        // new export
        //    group entry in which to put the IWO info.
        export.Group.Index = export.Group.Count;
        export.Group.CheckSize();

        export.Group.Update.G.Assign(import.NullCourtOrder);

        // --------------------------------------------------------------------------------------------------
        // DATA ELEMENT: Court Order
        // Report standard number of associated court orders (see Assumptions #5
        // for debt/collections, and IWO/Petition/Contempt rules below).  This
        // may be left blank if the only action is a petition created with no
        // court order number entered yet.
        // --------------------------------------------------------------------------------------------------
        export.Group.Update.G.CoCourtOrderNumber =
          Substring(entities.LegalAction.StandardNumber, 1, 12);
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Created Date of Last I Classification Legal Action
      // 1) As of the report run date.  Report created date of most recent (
      // created date)
      //    I Classification legal action for the court order.
      // 2) Report 01-01-0001 if none exists.
      // --------------------------------------------------------------------------------------------------
      local.DateWorkArea.Date = Date(entities.LegalAction.CreatedTstamp);
      export.Group.Update.G.CoLastIClassCreatedDate = UseCabFormatDate();

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Last I Classification Legal Action Taken
      // 1) As of the report run date.  Report action taken of the most recent (
      // created date)
      //    I Classification legal action for the court order.
      // 2) Report blank if none exists.
      // --------------------------------------------------------------------------------------------------
      export.Group.Update.G.CoLastIClassActionTaken =
        Substring(entities.LegalAction.ActionTaken, 1, 8);

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Filed Date of Last I Classification Legal Action
      // 1) As of the report run date.  Report filed date of the most recent (
      // created date)
      //    I Classification legal action for the court order.
      // 2) Report 01-01-0001 if none exists.
      // --------------------------------------------------------------------------------------------------
      if (Equal(entities.LegalAction.FiledDate, local.Null1.Date))
      {
        export.Group.Update.G.CoLastIClassFiledDate = local.NullTextDate.Text10;
      }
      else
      {
        local.DateWorkArea.Date = entities.LegalAction.FiledDate;
        export.Group.Update.G.CoLastIClassFiledDate = UseCabFormatDate();
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Last I Classification Legal Action IWGL
      // 1) As of the report run date.  Report Y if IWGL exists on the most 
      // recent (created date)
      //    I Classification legal action for the court order.
      // 2) Report N if none exists.
      // --------------------------------------------------------------------------------------------------
      if (ReadLegalActionIncomeSource())
      {
        export.Group.Update.G.CoLastIClassIwgl = "Y";
      }
      else
      {
        export.Group.Update.G.CoLastIClassIwgl = "N";
      }
    }

    local.Iwo.ActionTaken = "IWO";
    local.Iwomodo.ActionTaken = "IWOMODO";
    local.Iwonotks.ActionTaken = "IWONOTKS";
    local.Iwonotkm.ActionTaken = "IWONOTKM";
    local.Iwoterm.ActionTaken = "IWOTERM";
    local.Iwonotkt.ActionTaken = "IWONOTKT";
    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";

    foreach(var item in ReadLegalAction13())
    {
      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Monthly IWO Arrears Amount Due (WA)
      // 1) As of the report run date, find the most recent (Filed Date) IWO, 
      // IWOMODO, IWONOTKS,
      //    or IWONOTKM legal action, and report the WA amount from that order (
      // converted to monthly).
      //    Report 0.00 if no WA amount exists on the legal action.
      // 2) Ignore IWO, IWMODO, IWONOTKS, and IWONOTKM legal actions with no 
      // file date.
      // 3) If the most recent filed IWO, IWOMODO, IWONOTKS, or IWONOTKM legal 
      // action is end-dated
      //    (or if none exists) report 0.00.
      // 4) If there is a more recent created legal action that is an IWOTERM, 
      // IWONOTKT
      //    (termination created date is greater than the most recent IWO file 
      // date) , report 0.00.
      // --------------------------------------------------------------------------------------------------
      // -- Do not include the court order if this is not the most recently 
      // created I class legal action.
      if (ReadLegalAction2())
      {
        continue;
      }

      if (Lt(local.Null1.Date, entities.LegalAction.EndDate) && Lt
        (entities.LegalAction.EndDate, import.ProgramProcessingInfo.ProcessDate))
        
      {
        continue;
      }

      // -- Convert Filed Date to a timestamp value
      local.DateWorkArea.Date = entities.LegalAction.FiledDate;
      UseCabDate2TextWithHyphens();
      local.DateWorkArea.Timestamp = Timestamp(local.TextWorkArea.Text10);

      if (ReadLegalAction1())
      {
        continue;
      }

      if (ReadLegalActionDetail())
      {
        // -- Convert to yearly amount
        switch(TrimEnd(entities.IclassWaLegalActionDetail.FreqPeriodCode))
        {
          case "BW":
            local.Common.TotalCurrency =
              entities.IclassWaLegalActionDetail.ArrearsAmount.
                GetValueOrDefault() * 26;

            break;
          case "M":
            local.Common.TotalCurrency =
              entities.IclassWaLegalActionDetail.ArrearsAmount.
                GetValueOrDefault() * 12;

            break;
          case "SM":
            local.Common.TotalCurrency =
              entities.IclassWaLegalActionDetail.ArrearsAmount.
                GetValueOrDefault() * 24;

            break;
          case "W":
            local.Common.TotalCurrency =
              entities.IclassWaLegalActionDetail.ArrearsAmount.
                GetValueOrDefault() * 52;

            break;
          default:
            // -- Log to error report...
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Invalid Legal Detail Frequency Code " + entities
              .IclassWaLegalActionDetail.FreqPeriodCode + " found for standard number " +
              Substring
              (entities.LegalAction.StandardNumber,
              LegalAction.StandardNumber_MaxLength, 1, 12) + ".";
            UseCabErrorReport();

            continue;
        }

        // -- Check if this standard number is already in the export group.
        local.CourtOrderFound.Flag = "N";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.G.CoCourtOrderNumber,
            entities.LegalAction.StandardNumber, 1, 12))
          {
            local.CourtOrderFound.Flag = "Y";

            break;
          }
        }

        export.Group.CheckIndex();

        if (AsChar(local.CourtOrderFound.Flag) == 'Y')
        {
          // -- The standard number was already in the export group.  Leave the 
          // subscript alone,
          //    this is where we'll put the IWO info.
        }
        else
        {
          // -- The standard number was not already in the export group.  Create
          // a new export
          //    group entry in which to put the IWO info.
          export.Group.Index = export.Group.Count;
          export.Group.CheckSize();

          export.Group.Update.G.Assign(import.NullCourtOrder);

          // --------------------------------------------------------------------------------------------------
          // DATA ELEMENT: Court Order
          // Report standard number of associated court orders (see Assumptions 
          // #5 for debt/collections, and IWO/Petition/Contempt rules below).
          // This may be left blank if the only action is a petition created
          // with no court order number entered yet.
          // --------------------------------------------------------------------------------------------------
          export.Group.Update.G.CoCourtOrderNumber =
            Substring(entities.LegalAction.StandardNumber, 1, 12);
        }

        // -- Convert to monthly amount
        local.Common.TotalCurrency = local.Common.TotalCurrency / 12;
        export.Group.Update.G.CoMonthlyIwoWaAmount =
          UseFnB795ConvertNumToText();
      }
      else
      {
        continue;
      }
    }

    local.Contempt.ActionTaken = "CONTEMPT";
    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";

    foreach(var item in ReadLegalAction10())
    {
      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      // -- Skip the court order if this is not the most recently created 
      // CONTEMPT legal action.
      if (ReadLegalAction3())
      {
        continue;
      }

      // -- Check if this standard number is already in the export group.
      local.CourtOrderFound.Flag = "N";

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.G.CoCourtOrderNumber,
          entities.LegalAction.StandardNumber, 1, 12))
        {
          local.CourtOrderFound.Flag = "Y";

          break;
        }
      }

      export.Group.CheckIndex();

      if (AsChar(local.CourtOrderFound.Flag) == 'Y')
      {
        // -- The standard number was already in the export group.  Leave the 
        // subscript alone,
        //    this is where we'll put the IWO info.
      }
      else
      {
        // -- The standard number was not already in the export group.  Create a
        // new export
        //    group entry in which to put the IWO info.
        export.Group.Index = export.Group.Count;
        export.Group.CheckSize();

        export.Group.Update.G.Assign(import.NullCourtOrder);

        // --------------------------------------------------------------------------------------------------
        // DATA ELEMENT: Court Order
        // Report standard number of associated court orders (see Assumptions #5
        // for debt/collections, and IWO/Petition/Contempt rules below).  This
        // may be left blank if the only action is a petition created with no
        // court order number entered yet.
        // --------------------------------------------------------------------------------------------------
        export.Group.Update.G.CoCourtOrderNumber =
          Substring(entities.LegalAction.StandardNumber, 1, 12);
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Recent and Future Contempt Hearing Date
      // 1) As of the report run date, find the most recent (Created Date) 
      // CONTEMPT legal action
      //    for the court order.
      // 2) If any hearings are tied to the legal action through HEAR, report 
      // the most
      //    recent/greatest past or future Hearing Date from HEAR.
      // 3) Credit the NCP/case where the NCP is the active AP on the case, and 
      // the NCP is listed
      //    on LROL for the qualifying CONTEMPT legal action with an AP role and
      // with the same case
      //    number.
      // 4) If no CONTEMPT legal actions exist, or if no Hearing Date exists for
      // the most
      //    recent CONTEMPT legal action, report 01-01-0001.
      // --------------------------------------------------------------------------------------------------
      export.Group.Update.G.CoContemptHearingDate = local.NullTextDate.Text10;

      if (ReadHearing())
      {
        local.DateWorkArea.Date = entities.Hearing.ConductedDate;
        export.Group.Update.G.CoContemptHearingDate = UseCabFormatDate();
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Recent and Future Contempt Service Date
      // 1) As of the report run date, find the most recent (Created Date) 
      // CONTEMPT legal action
      //    for the court order.
      // 2) If any service records are tied to the legal action through SERV, 
      // report the most recent
      //    Service Date from SERV.
      // 3) Credit the NCP/case where the NCP is the active AP on the case, and 
      // the NCP is listed
      //    on LROL for the qualifying CONTEMPT legal action with an AP role and
      // with the same case
      //    number.
      // 4) If no CONTEMPT legal actions exist, or if no Service Date exists for
      // the most recent
      //    CONTEMPT legal action, report 01-01-0001.
      // --------------------------------------------------------------------------------------------------
      export.Group.Update.G.CoContemptServiceDate = local.NullTextDate.Text10;

      if (ReadServiceProcess())
      {
        if (Equal(entities.ServiceProcess.ServiceDate, local.Null1.Date))
        {
          export.Group.Update.G.CoContemptServiceDate =
            local.NullTextDate.Text10;
        }
        else
        {
          local.DateWorkArea.Date = entities.ServiceProcess.ServiceDate;
          export.Group.Update.G.CoContemptServiceDate = UseCabFormatDate();
        }
      }
    }

    // -- Note: legal_action identifier and infrastructure denorm_numeric_12 are
    // different lengths.
    //          Performance will suffer since the legal action identifier will 
    // have to be expanded
    //          from 9 to 12 before comparison but there doesn't appear to be an
    // easy way around
    //          this.  Might have to revisit this if performance becomes an 
    // issue.
    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";
    local.Event20.EventId = 20;
    local.Event22.EventId = 22;
    local.Demand1.ReasonCode = "DEMAND1";
    local.Demand1A.ReasonCode = "DEMAND1A";
    local.Demand2.ReasonCode = "DEMAND2";
    local.Demand2A.ReasonCode = "DEMAND2A";

    foreach(var item in ReadInfrastructureLegalAction())
    {
      if (!Equal(entities.Infrastructure.CaseNumber,
        import.ContractorCaseUniverse.CaseNumber))
      {
        continue;
      }

      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      // -- Check if this standard number is already in the export group.
      local.CourtOrderFound.Flag = "N";

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.G.CoCourtOrderNumber,
          entities.LegalAction.StandardNumber, 1, 12))
        {
          local.CourtOrderFound.Flag = "Y";

          break;
        }
      }

      export.Group.CheckIndex();

      if (AsChar(local.CourtOrderFound.Flag) == 'Y')
      {
        // -- The standard number was already in the export group.  Leave the 
        // subscript alone,
        //    this is where we'll put the IWO info.
      }
      else
      {
        // -- The standard number was not already in the export group.  Create a
        // new export
        //    group entry in which to put the IWO info.
        export.Group.Index = export.Group.Count;
        export.Group.CheckSize();

        export.Group.Update.G.Assign(import.NullCourtOrder);

        // --------------------------------------------------------------------------------------------------
        // DATA ELEMENT: Court Order
        // Report standard number of associated court orders (see Assumptions #5
        // for debt/collections, and IWO/Petition/Contempt rules below).  This
        // may be left blank if the only action is a petition created with no
        // court order number entered yet.
        // --------------------------------------------------------------------------------------------------
        export.Group.Update.G.CoCourtOrderNumber =
          Substring(entities.LegalAction.StandardNumber, 1, 12);
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Created Date of Last Demand Letter
      // 1) As of the report run date.  Report the created date of the most 
      // recent DEMAND1,
      //    DEMAND1A, DEMAND2, DEMAND2A document for the court order/NCP 
      // combination.
      // 2) Report 01-01-0001 if none.
      // 3) See assumption #5 to tie a demand letter to a case.
      // --------------------------------------------------------------------------------------------------
      local.DateWorkArea.Date = Date(entities.Infrastructure.CreatedTimestamp);
      export.Group.Update.G.CoDemandLetterCreatedDate = UseCabFormatDate();
    }

    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";

    foreach(var item in ReadLegalAction12())
    {
      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      if (IsEmpty(entities.LegalAction.StandardNumber))
      {
        // -- Skip the court order if this is not the most recently created P 
        // class legal action.
        // -- The approach below was dictated by performance problems when the 
        // read each was structured in any different manner.
        foreach(var item1 in ReadLegalActionLegalActionCaseRole1())
        {
          if (!IsEmpty(entities.Other.StandardNumber))
          {
            // -- We sorted the legal actions ascending by the standard number, 
            // so if the standard number is not
            // -- spaces then a more recently created P class action with a 
            // blank standard number does not exist.
            break;
          }

          if (AsChar(entities.Other.Classification) < 'P')
          {
            continue;
          }
          else if (AsChar(entities.Other.Classification) > 'P')
          {
            // -- We sorted the legal actions ascending by classification within
            // each standard number.
            //    Since the blank standard number will be retrieved first, if 
            // the classification is greater
            //    than P then a more recently created P class action with a 
            // blank standard number does
            //    not exist.
            break;
          }
          else if (AsChar(entities.Other.Classification) == 'P')
          {
            if (ReadLegalActionPerson())
            {
              // -- We found a more recently created P class action with a blank
              // standard number.
              goto ReadEach1;
            }
            else
            {
              // -- Continue.  Look for another P class legal action with a 
              // blank standard number tied to this case.
            }
          }
        }
      }
      else
      {
        // -- Skip the court order if this is not the most recently created P 
        // class legal action.
        if (ReadLegalAction5())
        {
          continue;
        }

        // -- Don't include if a J or O class legal action has been created 
        // after the petition.
        if (ReadLegalAction8())
        {
          continue;
        }
      }

      // -- Don't include if the petition is end dated.
      if (Lt(entities.LegalAction.EndDate,
        import.ProgramProcessingInfo.ProcessDate))
      {
        continue;
      }

      // -- Check if this standard number is already in the export group.
      local.CourtOrderFound.Flag = "N";

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.G.CoCourtOrderNumber,
          entities.LegalAction.StandardNumber, 1, 12))
        {
          local.CourtOrderFound.Flag = "Y";

          break;
        }
      }

      export.Group.CheckIndex();

      if (AsChar(local.CourtOrderFound.Flag) == 'Y')
      {
        // -- The standard number was already in the export group.  Leave the 
        // subscript alone,
        //    this is where we'll put the IWO info.
      }
      else
      {
        // -- The standard number was not already in the export group.  Create a
        // new export
        //    group entry in which to put the IWO info.
        export.Group.Index = export.Group.Count;
        export.Group.CheckSize();

        export.Group.Update.G.Assign(import.NullCourtOrder);

        // --------------------------------------------------------------------------------------------------
        // DATA ELEMENT: Court Order
        // Report standard number of associated court orders (see Assumptions #5
        // for debt/collections, and IWO/Petition/Contempt rules below).  This
        // may be left blank if the only action is a petition created with no
        // court order number entered yet.
        // --------------------------------------------------------------------------------------------------
        export.Group.Update.G.CoCourtOrderNumber =
          Substring(entities.LegalAction.StandardNumber, 1, 12);
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Petition Created Date
      // 1) As of the report run date.  Report Created Date of the most recent (
      // created date) non-end
      //    dated P class legal action for the court order if no subsequent J/
      // O legal action exists.
      //    If no court order exists for the petition, it will report for the 
      // associated case(s)
      //    without a court order listed.
      // 2) Report 01-01-0001 if none.
      // 3) Use LROL AP/Case to tie the legal action to the case(s) (same as in 
      // Assumption #5).
      // --------------------------------------------------------------------------------------------------
      local.DateWorkArea.Date = Date(entities.LegalAction.CreatedTstamp);
      export.Group.Update.G.CoPetitionCreatedDate = UseCabFormatDate();

ReadEach1:
      ;
    }

    local.Previous.StandardNumber = "ZZZZZZZZZZZZ";

    foreach(var item in ReadLegalAction14())
    {
      if (Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        continue;
      }

      local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

      if (IsEmpty(entities.LegalAction.StandardNumber))
      {
        // -- Skip the court order if this is not the most recently filed P 
        // class legal action.
        // -- The approach below was dictated by performance problems when the 
        // read each was structured in any different manner.
        foreach(var item1 in ReadLegalActionLegalActionCaseRole2())
        {
          if (!IsEmpty(entities.Other.StandardNumber))
          {
            // -- We sorted the legal actions ascending by the standard number, 
            // so if the standard number is not
            // -- spaces then a more recently created P class action with a 
            // blank standard number does not exist.
            break;
          }

          if (AsChar(entities.Other.Classification) < 'P')
          {
            continue;
          }
          else if (AsChar(entities.Other.Classification) > 'P')
          {
            // -- We sorted the legal actions ascending by classification within
            // each standard number.
            //    Since the blank standard number will be retrieved first, if 
            // the classification is greater
            //    than P then a more recently created P class action with a 
            // blank standard number does
            //    not exist.
            break;
          }
          else if (AsChar(entities.Other.Classification) == 'P')
          {
            if (ReadLegalActionPerson())
            {
              // -- We found a more recently created P class action with a blank
              // standard number.
              goto ReadEach2;
            }
            else
            {
              // -- Continue.  Look for another P class legal action with a 
              // blank standard number tied to this case.
            }
          }
        }
      }
      else
      {
        // -- Skip the court order if this is not the most recently filed P 
        // class legal action.
        if (ReadLegalAction6())
        {
          continue;
        }

        // -- Convert Filed Date to a timestamp value
        local.DateWorkArea.Date = entities.LegalAction.FiledDate;
        UseCabDate2TextWithHyphens();
        local.DateWorkArea.Timestamp = Timestamp(local.TextWorkArea.Text10);

        // -- Don't include if a J or O class legal action has been created 
        // after the petition filed date.
        if (ReadLegalAction7())
        {
          continue;
        }
      }

      // -- Don't include if the petition is end dated.
      if (Lt(entities.LegalAction.EndDate,
        import.ProgramProcessingInfo.ProcessDate))
      {
        continue;
      }

      // -- Check if this standard number is already in the export group.
      local.CourtOrderFound.Flag = "N";

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.G.CoCourtOrderNumber,
          entities.LegalAction.StandardNumber, 1, 12))
        {
          local.CourtOrderFound.Flag = "Y";

          break;
        }
      }

      export.Group.CheckIndex();

      if (AsChar(local.CourtOrderFound.Flag) == 'Y')
      {
        // -- The standard number was already in the export group.  Leave the 
        // subscript alone,
        //    this is where we'll put the IWO info.
      }
      else
      {
        // -- The standard number was not already in the export group.  Create a
        // new export
        //    group entry in which to put the IWO info.
        export.Group.Index = export.Group.Count;
        export.Group.CheckSize();

        export.Group.Update.G.Assign(import.NullCourtOrder);

        // --------------------------------------------------------------------------------------------------
        // DATA ELEMENT: Court Order
        // Report standard number of associated court orders (see Assumptions #5
        // for debt/collections, and IWO/Petition/Contempt rules below).  This
        // may be left blank if the only action is a petition created with no
        // court order number entered yet.
        // --------------------------------------------------------------------------------------------------
        export.Group.Update.G.CoCourtOrderNumber =
          Substring(entities.LegalAction.StandardNumber, 1, 12);
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Petition Filed Date
      // 1) As of the report run date.  Report the Filed Date of the most recent
      // (filed date) non-end
      //    P class legal action for the court order if no subsequent J/O 
      // legal action exists.
      //    If no court order exists for the petition, it will report for the 
      // associated case(s)
      //    without a court order listed.
      // 2) Report 01-01-0001 if none.
      // 3) Use LROL AP/Case to tie the legal action to the case(s) (same as in 
      // Assumption #5).
      // --------------------------------------------------------------------------------------------------
      if (Equal(entities.LegalAction.FiledDate, local.Null1.Date))
      {
        export.Group.Update.G.CoPetitionFiledDate = local.NullTextDate.Text10;
      }
      else
      {
        local.DateWorkArea.Date = entities.LegalAction.FiledDate;
        export.Group.Update.G.CoPetitionFiledDate = UseCabFormatDate();
      }

ReadEach2:
      ;
    }
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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

  private string UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private string UseFnB795ConvertNumToText()
  {
    var useImport = new FnB795ConvertNumToText.Import();
    var useExport = new FnB795ConvertNumToText.Export();

    useImport.Common.TotalCurrency = local.Common.TotalCurrency;

    Call(FnB795ConvertNumToText.Execute, useImport, useExport);

    return useExport.WorkArea.Text15;
  }

  private bool ReadCashReceiptDetailCollectionType1()
  {
    entities.CollectionType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetNullableString(
          command, "courtOrderNumber", entities.LegalAction.StandardNumber ?? ""
          );
        db.SetInt32(
          command, "sequentialIdentifier1", local.F1.SequentialIdentifier);
        db.SetInt32(
          command, "sequentialIdentifier2", local.S.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 8);
        entities.CollectionType.Code = db.GetString(reader, 9);
        entities.CollectionType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType2()
  {
    entities.CollectionType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetInt32(
          command, "sequentialIdentifier1", local.F1.SequentialIdentifier);
        db.SetInt32(
          command, "sequentialIdentifier2", local.S.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 8);
        entities.CollectionType.Code = db.GetString(reader, 9);
        entities.CollectionType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadDashboardAuditData1()
  {
    return Read("ReadDashboardAuditData1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetString(
          command, "dashboardPriority",
          local.DashboardAuditData.DashboardPriority);
        db.SetNullableString(
          command, "payorCspNumber",
          import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetNullableString(
          command, "standardNumber", entities.LegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        local.Common.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadDashboardAuditData2()
  {
    return Read("ReadDashboardAuditData2",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetNullableString(
          command, "payorCspNumber",
          import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetNullableString(
          command, "standardNumber", entities.LegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        local.Common.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadDashboardAuditData3()
  {
    return Read("ReadDashboardAuditData3",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetString(
          command, "dashboardPriority",
          local.DashboardAuditData.DashboardPriority);
        db.SetNullableString(
          command, "payorCspNumber",
          import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetNullableString(
          command, "standardNumber", entities.LegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        local.Common.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureLegalAction()
  {
    entities.Infrastructure.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadInfrastructureLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum",
          import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetInt32(command, "eventId1", local.Event20.EventId);
        db.SetInt32(command, "eventId2", local.Event22.EventId);
        db.SetString(command, "reasonCode1", local.Demand1.ReasonCode);
        db.SetString(command, "reasonCode2", local.Demand1A.ReasonCode);
        db.SetString(command, "reasonCode3", local.Demand2.ReasonCode);
        db.SetString(command, "reasonCode4", local.Demand2A.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 3);
        entities.LegalAction.Identifier = db.GetInt32(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.ActionTaken = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 12);
        entities.Infrastructure.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.IclassTerm.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification", local.I.Classification);
        db.SetString(command, "actionTaken1", local.Iwoterm.ActionTaken);
        db.SetString(command, "actionTaken2", local.Iwonotkt.ActionTaken);
        db.SetDateTime(
          command, "createdTstamp",
          local.DateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IclassTerm.Identifier = db.GetInt32(reader, 0);
        entities.IclassTerm.Classification = db.GetString(reader, 1);
        entities.IclassTerm.ActionTaken = db.GetString(reader, 2);
        entities.IclassTerm.FiledDate = db.GetNullableDate(reader, 3);
        entities.IclassTerm.EndDate = db.GetNullableDate(reader, 4);
        entities.IclassTerm.StandardNumber = db.GetNullableString(reader, 5);
        entities.IclassTerm.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.IclassTerm.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction10()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction10",
      (db, command) =>
      {
        db.SetString(command, "classification", local.M.Classification);
        db.SetString(command, "actionTaken", local.Contempt.ActionTaken);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetString(command, "croType", local.Ap.Type1);
        db.
          SetString(command, "casNum", import.ContractorCaseUniverse.CaseNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction11()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction11",
      (db, command) =>
      {
        db.SetString(command, "classification", local.I.Classification);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetString(command, "croType", local.Ap.Type1);
        db.
          SetString(command, "casNum", import.ContractorCaseUniverse.CaseNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction12()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction12",
      (db, command) =>
      {
        db.SetString(command, "classification", local.P.Classification);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetString(command, "croType", local.Ap.Type1);
        db.
          SetString(command, "casNum", import.ContractorCaseUniverse.CaseNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction13()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction13",
      (db, command) =>
      {
        db.SetString(command, "classification", local.I.Classification);
        db.SetString(command, "actionTaken1", local.Iwo.ActionTaken);
        db.SetString(command, "actionTaken2", local.Iwomodo.ActionTaken);
        db.SetString(command, "actionTaken3", local.Iwonotks.ActionTaken);
        db.SetString(command, "actionTaken4", local.Iwonotkm.ActionTaken);
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetString(command, "croType", local.Ap.Type1);
        db.
          SetString(command, "casNum", import.ContractorCaseUniverse.CaseNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction14()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction14",
      (db, command) =>
      {
        db.SetString(command, "classification", local.P.Classification);
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetString(command, "croType", local.Ap.Type1);
        db.
          SetString(command, "casNum", import.ContractorCaseUniverse.CaseNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification", local.I.Classification);
        db.SetString(command, "actionTaken1", local.Iwo.ActionTaken);
        db.SetString(command, "actionTaken2", local.Iwomodo.ActionTaken);
        db.SetString(command, "actionTaken3", local.Iwonotks.ActionTaken);
        db.SetString(command, "actionTaken4", local.Iwonotkm.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          entities.LegalAction.FiledDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification", local.M.Classification);
        db.SetString(command, "actionTaken", local.Contempt.ActionTaken);
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private bool ReadLegalAction4()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification", local.I.Classification);
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private bool ReadLegalAction5()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification", local.P.Classification);
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private bool ReadLegalAction6()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction6",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification", local.P.Classification);
        db.SetNullableDate(
          command, "filedDt",
          entities.LegalAction.FiledDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private bool ReadLegalAction7()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction7",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification1", local.J.Classification);
        db.SetString(command, "classification2", local.O.Classification);
        db.SetDateTime(
          command, "createdTstamp",
          local.DateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private bool ReadLegalAction8()
  {
    entities.Other.Populated = false;

    return Read("ReadLegalAction8",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetString(command, "classification1", local.J.Classification);
        db.SetString(command, "classification2", local.O.Classification);
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Other.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction9()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction9",
      (db, command) =>
      {
        db.SetString(command, "classification1", local.J.Classification);
        db.SetString(command, "classification2", local.O.Classification);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.ContractorCaseUniverse.NcpPersonNumber);
        db.SetString(command, "croType", local.Ap.Type1);
        db.
          SetString(command, "casNum", import.ContractorCaseUniverse.CaseNumber);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.IclassWaLegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetString(command, "debtTypCd", local.Wa.Code);
      },
      (db, reader) =>
      {
        entities.IclassWaLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 0);
        entities.IclassWaLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.IclassWaLegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 2);
        entities.IclassWaLegalActionDetail.DetailType = db.GetString(reader, 3);
        entities.IclassWaLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 4);
        entities.IclassWaLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 5);
        entities.IclassWaLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.IclassWaLegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionIncomeSource()
  {
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 3);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionIncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionCaseRole1()
  {
    entities.Other.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "croType", local.Ap.Type1);
        db.SetString(
          command, "casNumber", import.ContractorCaseUniverse.CaseNumber);
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 6);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 8);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 9);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 10);
        entities.Other.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionCaseRole2()
  {
    entities.Other.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "croType", local.Ap.Type1);
        db.SetString(
          command, "casNumber", import.ContractorCaseUniverse.CaseNumber);
        db.SetNullableDate(
          command, "filedDt",
          entities.LegalAction.FiledDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 0);
        entities.Other.Classification = db.GetString(reader, 1);
        entities.Other.ActionTaken = db.GetString(reader, 2);
        entities.Other.FiledDate = db.GetNullableDate(reader, 3);
        entities.Other.StandardNumber = db.GetNullableString(reader, 4);
        entities.Other.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 6);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 8);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 9);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 10);
        entities.Other.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.ServiceProcess.Populated = true;
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
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of ContractorCaseUniverse.
    /// </summary>
    [JsonPropertyName("contractorCaseUniverse")]
    public ContractorCaseUniverse ContractorCaseUniverse
    {
      get => contractorCaseUniverse ??= new();
      set => contractorCaseUniverse = value;
    }

    /// <summary>
    /// A value of NullCourtOrder.
    /// </summary>
    [JsonPropertyName("nullCourtOrder")]
    public ContractorCaseUniverse NullCourtOrder
    {
      get => nullCourtOrder ??= new();
      set => nullCourtOrder = value;
    }

    private DashboardAuditData dashboardAuditData;
    private ProgramProcessingInfo programProcessingInfo;
    private ContractorCaseUniverse contractorCaseUniverse;
    private ContractorCaseUniverse nullCourtOrder;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public ContractorCaseUniverse G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private ContractorCaseUniverse g;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Event22.
    /// </summary>
    [JsonPropertyName("event22")]
    public Infrastructure Event22
    {
      get => event22 ??= new();
      set => event22 = value;
    }

    /// <summary>
    /// A value of Event20.
    /// </summary>
    [JsonPropertyName("event20")]
    public Infrastructure Event20
    {
      get => event20 ??= new();
      set => event20 = value;
    }

    /// <summary>
    /// A value of Demand2A.
    /// </summary>
    [JsonPropertyName("demand2A")]
    public Infrastructure Demand2A
    {
      get => demand2A ??= new();
      set => demand2A = value;
    }

    /// <summary>
    /// A value of Demand2.
    /// </summary>
    [JsonPropertyName("demand2")]
    public Infrastructure Demand2
    {
      get => demand2 ??= new();
      set => demand2 = value;
    }

    /// <summary>
    /// A value of Demand1A.
    /// </summary>
    [JsonPropertyName("demand1A")]
    public Infrastructure Demand1A
    {
      get => demand1A ??= new();
      set => demand1A = value;
    }

    /// <summary>
    /// A value of Demand1.
    /// </summary>
    [JsonPropertyName("demand1")]
    public Infrastructure Demand1
    {
      get => demand1 ??= new();
      set => demand1 = value;
    }

    /// <summary>
    /// A value of Distinct.
    /// </summary>
    [JsonPropertyName("distinct")]
    public LegalAction Distinct
    {
      get => distinct ??= new();
      set => distinct = value;
    }

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public LegalAction P
    {
      get => p ??= new();
      set => p = value;
    }

    /// <summary>
    /// A value of M.
    /// </summary>
    [JsonPropertyName("m")]
    public LegalAction M
    {
      get => m ??= new();
      set => m = value;
    }

    /// <summary>
    /// A value of Wa.
    /// </summary>
    [JsonPropertyName("wa")]
    public ObligationType Wa
    {
      get => wa ??= new();
      set => wa = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public LegalAction I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// A value of S.
    /// </summary>
    [JsonPropertyName("s")]
    public CollectionType S
    {
      get => s ??= new();
      set => s = value;
    }

    /// <summary>
    /// A value of F1.
    /// </summary>
    [JsonPropertyName("f1")]
    public CollectionType F1
    {
      get => f1 ??= new();
      set => f1 = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of O.
    /// </summary>
    [JsonPropertyName("o")]
    public LegalAction O
    {
      get => o ??= new();
      set => o = value;
    }

    /// <summary>
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public LegalAction J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of Contempt.
    /// </summary>
    [JsonPropertyName("contempt")]
    public LegalAction Contempt
    {
      get => contempt ??= new();
      set => contempt = value;
    }

    /// <summary>
    /// A value of Iwoterm.
    /// </summary>
    [JsonPropertyName("iwoterm")]
    public LegalAction Iwoterm
    {
      get => iwoterm ??= new();
      set => iwoterm = value;
    }

    /// <summary>
    /// A value of Iwonotkt.
    /// </summary>
    [JsonPropertyName("iwonotkt")]
    public LegalAction Iwonotkt
    {
      get => iwonotkt ??= new();
      set => iwonotkt = value;
    }

    /// <summary>
    /// A value of Iwonotkm.
    /// </summary>
    [JsonPropertyName("iwonotkm")]
    public LegalAction Iwonotkm
    {
      get => iwonotkm ??= new();
      set => iwonotkm = value;
    }

    /// <summary>
    /// A value of Iwonotks.
    /// </summary>
    [JsonPropertyName("iwonotks")]
    public LegalAction Iwonotks
    {
      get => iwonotks ??= new();
      set => iwonotks = value;
    }

    /// <summary>
    /// A value of Iwomodo.
    /// </summary>
    [JsonPropertyName("iwomodo")]
    public LegalAction Iwomodo
    {
      get => iwomodo ??= new();
      set => iwomodo = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public LegalAction Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of NullCurrency.
    /// </summary>
    [JsonPropertyName("nullCurrency")]
    public WorkArea NullCurrency
    {
      get => nullCurrency ??= new();
      set => nullCurrency = value;
    }

    /// <summary>
    /// A value of CourtOrderFound.
    /// </summary>
    [JsonPropertyName("courtOrderFound")]
    public Common CourtOrderFound
    {
      get => courtOrderFound ??= new();
      set => courtOrderFound = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of NullTextDate.
    /// </summary>
    [JsonPropertyName("nullTextDate")]
    public TextWorkArea NullTextDate
    {
      get => nullTextDate ??= new();
      set => nullTextDate = value;
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

    private Infrastructure event22;
    private Infrastructure event20;
    private Infrastructure demand2A;
    private Infrastructure demand2;
    private Infrastructure demand1A;
    private Infrastructure demand1;
    private LegalAction distinct;
    private LegalAction p;
    private LegalAction m;
    private ObligationType wa;
    private LegalAction i;
    private CollectionType s;
    private CollectionType f1;
    private CaseRole ap;
    private LegalAction o;
    private LegalAction j;
    private LegalAction contempt;
    private LegalAction iwoterm;
    private LegalAction iwonotkt;
    private LegalAction iwonotkm;
    private LegalAction iwonotks;
    private LegalAction iwomodo;
    private LegalAction iwo;
    private DashboardAuditData dashboardAuditData;
    private TextWorkArea textWorkArea;
    private LegalAction legalAction;
    private WorkArea nullCurrency;
    private Common courtOrderFound;
    private Common common;
    private DateWorkArea null1;
    private DateWorkArea dateWorkArea;
    private LegalAction previous;
    private TextWorkArea nullTextDate;
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
    /// A value of Distinct.
    /// </summary>
    [JsonPropertyName("distinct")]
    public LegalAction Distinct
    {
      get => distinct ??= new();
      set => distinct = value;
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
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public LegalAction Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
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
    /// A value of IclassTerm.
    /// </summary>
    [JsonPropertyName("iclassTerm")]
    public LegalAction IclassTerm
    {
      get => iclassTerm ??= new();
      set => iclassTerm = value;
    }

    /// <summary>
    /// A value of IclassWaLegalActionDetail.
    /// </summary>
    [JsonPropertyName("iclassWaLegalActionDetail")]
    public LegalActionDetail IclassWaLegalActionDetail
    {
      get => iclassWaLegalActionDetail ??= new();
      set => iclassWaLegalActionDetail = value;
    }

    /// <summary>
    /// A value of IclassWaLegalAction.
    /// </summary>
    [JsonPropertyName("iclassWaLegalAction")]
    public LegalAction IclassWaLegalAction
    {
      get => iclassWaLegalAction ??= new();
      set => iclassWaLegalAction = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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

    private LegalAction distinct;
    private Infrastructure infrastructure;
    private LegalAction other;
    private ServiceProcess serviceProcess;
    private Hearing hearing;
    private ObligationType obligationType;
    private LegalAction iclassTerm;
    private LegalActionDetail iclassWaLegalActionDetail;
    private LegalAction iclassWaLegalAction;
    private LegalActionIncomeSource legalActionIncomeSource;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private DashboardAuditData dashboardAuditData;
    private CsePerson csePerson;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
